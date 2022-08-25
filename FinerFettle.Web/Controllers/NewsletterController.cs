using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using FinerFettle.Web.Extensions;
using FinerFettle.Web.Models.Newsletter;
using System.Linq;

namespace FinerFettle.Web.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly CoreContext _context;

        /// <summary>
        /// The name of the controller for routing purposes
        /// </summary>
        public const string Name = "Newsletter";

        public NewsletterController(CoreContext context)
        {
            _context = context;
        }

        [Route("newsletter/{email}")]
        public async Task<IActionResult> Newsletter(string email)
        {
            // TODO: Refactor

            var today = DateOnly.FromDateTime(DateTime.Today);

            var user = await _context.Users
                .Include(u => u.EquipmentUsers)
                .ThenInclude(u => u.Equipment)
                .Include(u => u.ExerciseProgressions)
                .FirstAsync(u => u.Email == email);
                
            if (user.NeedsRest)
            {
                user.NeedsRest = false;
                _context.Update(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            if (user.RestDays.HasFlag(RestDaysExtensions.FromDate(today)) == true)
            {
                return NoContent();
            }

            var todoExerciseType = new ExerciseTypeGroups(user.StrengtheningPreference).First(); // Have to start somewhere
            var previousNewsletter = await _context.Newsletters
                .Where(n => n.User == user)
                .OrderBy(n => n.Date)
                .ThenBy(n => n.Id) // Really just for testing. When two newsletters get sent in the same day, I want a difference exercise set.
                .LastOrDefaultAsync();

            if (previousNewsletter != null)
            {
                todoExerciseType = new ExerciseTypeGroups(user.StrengtheningPreference)
                    .SkipWhile(r => r != previousNewsletter.ExerciseRotation)
                    .Skip(1)
                    .FirstOrDefault() ?? todoExerciseType;
            }

            var newsletter = new Newsletter()
            {
                Date = today,
                User = user,
                ExerciseRotation = todoExerciseType
            };
            _context.Newsletters.Add(newsletter);
            await _context.SaveChangesAsync();

            var equipment = user.EquipmentUsers.Select(e => e.EquipmentId) ?? new List<int>();
            var averageProgression = user.ExerciseProgressions.Any() ? user.ExerciseProgressions.Average(p => p.Progression) : 50;

            // Flatten all exercise variations and intensities into one big list
            var allExercises = await _context.Variations
                .Include(v => v.Exercise)
                .ThenInclude(e => e.UserProgressions)
                .Where(v => v.Enabled)
                // Make sure the user owns all the equipment necessary for the exercise
                .Where(v => v.EquipmentGroups.All(g => g.Equipment.Any(eq => equipment.Contains(eq.Id))))
                .SelectMany(v => v.Intensities
                    // Select the current progression of each exercise.
                    // Using averageProgression as a hard-cap so that users can't get stuck without an exercise if they never see it because they are under the exercise's min progression
                    .Where(i =>
                        (Math.Max(averageProgression, v.Exercise.UserProgressions.First(up => up.User == user).Progression) >= i.MinProgression || i.MinProgression == null)
                        && (Math.Min(averageProgression, v.Exercise.UserProgressions.First(up => up.User == user).Progression) < i.MaxProgression || i.MaxProgression == null)
                    )
                    .Select(i => new {
                        Variation = v,
                        Intensity = i, // Need to select into an anonymous object so Proficiency is included...
                        Muscles = v.Exercise.Muscles,
                        ExerciseType = v.Exercise.ExerciseType
                    }))
                .Select(a => new ExerciseViewModel(user, a.Variation, a.Intensity, a.Muscles, a.ExerciseType, null))
                .OrderBy(_ => Guid.NewGuid()) // Select a random subset of exercises
                .ToListAsync();

            var exercises = allExercises
                // Make sure the exercise is the correct type and not a warmup exercise
                .Where(e => e.ActivityLevel == ExerciseActivityLevel.Main)
                .Where(e => todoExerciseType.ExerciseType.HasAnyFlag32(e.ExerciseType))
                .Aggregate(new List<ExerciseViewModel>(), (acc, e) => (
                    // Make sure the exercise covers a unique muscle group.
                    // This unsets the muscles worked in already selected exercises
                    // and then checks if the unique muscles contain a muscle that we want to target for the day.
                    // This will also prevent us from selecting two variations of the same exercise, since those cover the same muscle groups.
                    e.Muscles.UnsetFlag32(acc.Aggregate((MuscleGroups)0, (f, x) => f | x.Muscles)).HasAnyFlag32(todoExerciseType.MuscleGroups)
                ) ? new List<ExerciseViewModel>(acc) { e } : acc);

            foreach (var exercise in exercises)
            {
                // Each day works part of the body, not the full body. Work each muscle harder.
                exercise.Intensity.Proficiency.Sets += (int)user.StrengtheningPreference;
            }

            var viewModel = new NewsletterViewModel(exercises)
            {
                User = user,
                ExerciseType = todoExerciseType.ExerciseType,
                MuscleGroups = todoExerciseType.MuscleGroups
            };

            if (todoExerciseType.ExerciseType.HasAnyFlag32(ExerciseType.Cardio | ExerciseType.Strength))
            {
                viewModel.WarmupExercises = allExercises
                    // Choose dynamic stretches for warmup
                    .Where(e => e.ActivityLevel == ExerciseActivityLevel.Warmup)
                    .Aggregate(new List<ExerciseViewModel>(), (acc, e) => (
                        // Make sure the exercise covers a unique muscle group.
                        // This unsets the muscles worked in already selected exercises
                        // and then checks if the unique muscles contain a muscle that we want to target for the day.
                        // This will also prevent us from selecting two variations of the same exercise, since those cover the same muscle groups.
                        e.Muscles.UnsetFlag32(acc.Aggregate((MuscleGroups)0, (f, x) => f | x.Muscles)).HasAnyFlag32(todoExerciseType.MuscleGroups)
                    ) ? new List<ExerciseViewModel>(acc) { e } : acc);

                viewModel.CooldownExercises = allExercises
                    // Choose static stretches for cooldown
                    .Where(e => e.ActivityLevel == ExerciseActivityLevel.Cooldown)
                    .Aggregate(new List<ExerciseViewModel>(), (acc, e) => (
                        // Make sure the exercise covers a unique muscle group.
                        // This unsets the muscles worked in already selected exercises
                        // and then checks if the unique muscles contain a muscle that we want to target for the day.
                        e.Muscles.UnsetFlag32(acc.Aggregate((MuscleGroups)0, (f, x) => f | x.Muscles)).HasAnyFlag32(todoExerciseType.MuscleGroups)
                    ) ? new List<ExerciseViewModel>(acc) { e } : acc);
            }

            return View(nameof(Newsletter), viewModel);
        }
    }
}
