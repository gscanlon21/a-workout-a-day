using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using FinerFettle.Web.Extensions;
using FinerFettle.Web.Models.Newsletter;
using System.Numerics;

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
        public async Task<IActionResult> Newsletter(string email, bool demo = false)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var user = await _context.Users
                .Include(u => u.ExerciseProgressions)
                .Include(u => u.EquipmentUsers)
                    .ThenInclude(u => u.Equipment)
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
                .ThenBy(n => n.Id) // For testing/demo. When two newsletters get sent in the same day, I want a difference exercise set.
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

            // Flatten all exercise variations and intensities into one big list
            var allExercises = (await _context.Intensities
                .Include(v => v.Variation)
                    .ThenInclude(e => e.Exercise)
                        .ThenInclude(e => e.UserProgressions)
                .Include(i => i.EquipmentGroups)
                    .ThenInclude(eg => eg.Equipment)
                .Where(i => i.Variation.DisabledReason == null)
                // Select the current progression of each exercise.
                // Using averageProgression as a boost so that users can't get stuck without an exercise if they never see it because they are under the exercise's min progression
                .Where(i => i.Progression.Min == null || (5 * (int)Math.Floor((user.AverageProgression + i.Variation.Exercise.UserProgressions.First(up => up.User == user).Progression) / 10d)) >= i.Progression.Min)
                .Where(i => i.Progression.Max == null || (5 * (int)Math.Ceiling((user.AverageProgression + i.Variation.Exercise.UserProgressions.First(up => up.User == user).Progression) / 10d)) < i.Progression.Max)
                // Make sure the user owns all the equipment necessary for the exercise
                .Where(i => i.EquipmentGroups.All(g => g.Required == false || g.Equipment.Any(eq => user.EquipmentIds.Contains(eq.Id))))
                .Select(i => new ExerciseViewModel(user, i.Variation.Exercise, i.Variation, i))
                .ToListAsync())
                // Select a random subset of exercises
                .OrderBy(_ => Guid.NewGuid()); // OrderBy must come after query or you get duplicates.

            var exercises = allExercises
                .Where(vm => vm.ActivityLevel == ExerciseActivityLevel.Main)
                .Where(vm => todoExerciseType.ExerciseType.HasAnyFlag32(vm.Exercise.ExerciseType))
                .Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                    // Make sure the exercise covers a unique muscle group.
                    // This unsets the muscles worked in already selected exercises
                    // and then checks if the unique muscles contain a muscle that we want to target for the day.
                    // This will also prevent us from selecting two variations of the same exercise, since those cover the same muscle groups.
                    vm.Exercise.Muscles.UnsetFlag32(vms.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.Muscles)).HasAnyFlag32(todoExerciseType.MuscleGroups)
                    && (
                        // Choose either isolation exercises
                        BitOperations.PopCount((ulong)vm.Exercise.Muscles) == 1
                        // Or compound exercises that cover at least two muscles in the targeted muscles set
                        || BitOperations.PopCount((ulong)vm.Exercise.Muscles.UnsetFlag32(vms.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.Muscles)).UnsetFlag32(todoExerciseType.MuscleGroups)) <= (BitOperations.PopCount((ulong)vm.Exercise.Muscles) - 2)
                    )) ? new List<ExerciseViewModel>(vms) { vm } : vms);

            exercises = exercises.Concat(exercises.Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                    // Grab any muscle groups we missed in the previous aggregate
                    vm.Exercise.Muscles.UnsetFlag32(vms.Aggregate(exercises.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.Muscles), (m, vm2) => m | vm2.Exercise.Muscles)).HasAnyFlag32(todoExerciseType.MuscleGroups)
                    ) ? new List<ExerciseViewModel>(vms) { vm } : vms)).ToList();

            foreach (var exercise in exercises)
            {
                // Each day works part of the body, not the full body. Work each muscle harder.
                exercise.Intensity.Proficiency.Sets += (int)user.StrengtheningPreference;
            }

            var viewModel = new NewsletterViewModel(exercises, user)
            {
                ExerciseType = todoExerciseType.ExerciseType,
                MuscleGroups = todoExerciseType.MuscleGroups,
                Demo = demo
            };

            if (todoExerciseType.ExerciseType.HasAnyFlag32(ExerciseType.Cardio | ExerciseType.Strength))
            {
                viewModel.WarmupExercises = allExercises
                    .Where(vm => vm.ActivityLevel == ExerciseActivityLevel.Warmup)
                    .Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                        // Make sure the exercise covers a unique muscle group.
                        // This unsets the muscles worked in already selected exercises
                        // and then checks if the unique muscles contain a muscle that we want to target for the day.
                        // This will also prevent us from selecting two variations of the same exercise, since those cover the same muscle groups.
                        vm.Exercise.Muscles.UnsetFlag32(vms.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.Muscles)).HasAnyFlag32(todoExerciseType.MuscleGroups)
                        && (
                            // Choose either isolation exercises
                            BitOperations.PopCount((ulong)vm.Exercise.Muscles) == 1
                            // Or compound exercises that cover at least two muscles in the targeted muscles set
                            || BitOperations.PopCount((ulong)vm.Exercise.Muscles.UnsetFlag32(vms.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.Muscles)).UnsetFlag32(todoExerciseType.MuscleGroups)) <= (BitOperations.PopCount((ulong)vm.Exercise.Muscles) - 2)
                        )) ? new List<ExerciseViewModel>(vms) { vm } : vms);

                viewModel.WarmupExercises = viewModel.WarmupExercises.Concat(viewModel.WarmupExercises.Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                        // Grab any muscle groups we missed in the previous aggregate
                        vm.Exercise.Muscles.UnsetFlag32(vms.Aggregate(viewModel.WarmupExercises.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.Muscles), (m, vm2) => m | vm2.Exercise.Muscles)).HasAnyFlag32(todoExerciseType.MuscleGroups)
                        ) ? new List<ExerciseViewModel>(vms) { vm } : vms)).ToList();


                viewModel.CooldownExercises = allExercises
                    .Where(vm => vm.ActivityLevel == ExerciseActivityLevel.Cooldown)
                    .Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                        // Make sure the exercise covers a unique muscle group.
                        // This unsets the muscles worked in already selected exercises
                        // and then checks if the unique muscles contain a muscle that we want to target for the day.
                        vm.Exercise.Muscles.UnsetFlag32(vms.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.Muscles)).HasAnyFlag32(todoExerciseType.MuscleGroups)
                        && (
                            // Choose either isolation exercises
                            BitOperations.PopCount((ulong)vm.Exercise.Muscles) == 1
                            // Or compound exercises that cover at least two muscles in the targeted muscles set
                            || BitOperations.PopCount((ulong)vm.Exercise.Muscles.UnsetFlag32(vms.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.Muscles)).UnsetFlag32(todoExerciseType.MuscleGroups)) <= (BitOperations.PopCount((ulong)vm.Exercise.Muscles) - 2)
                        )) ? new List<ExerciseViewModel>(vms) { vm } : vms);

                viewModel.CooldownExercises = viewModel.CooldownExercises.Concat(viewModel.CooldownExercises.Aggregate(new List<ExerciseViewModel>(), (vms, vm) => (
                        // Grab any muscle groups we missed in the previous aggregate
                        vm.Exercise.Muscles.UnsetFlag32(vms.Aggregate(viewModel.CooldownExercises.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.Muscles), (m, vm2) => m | vm2.Exercise.Muscles)).HasAnyFlag32(todoExerciseType.MuscleGroups)
                        ) ? new List<ExerciseViewModel>(vms) { vm } : vms)).ToList();
            }

            return View(nameof(Newsletter), viewModel);
        }
    }
}
