using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using FinerFettle.Web.Extensions;
using FinerFettle.Web.Models.Newsletter;

namespace FinerFettle.Web.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly CoreContext _context;

        /// <summary>
        /// The name of the controller for routing purposes
        /// </summary>
        public const string Name = "Newsletter";

        /// <summary>
        /// Muscle groups to work out together
        /// </summary>
        public static readonly IList<MuscleGroups> MuscleGroupings = new List<MuscleGroups>(5) {
            Models.Exercise.MuscleGroupings.UpperBodyPull,
            Models.Exercise.MuscleGroupings.UpperBodyPush,
            Models.Exercise.MuscleGroupings.MidBody,
            Models.Exercise.MuscleGroupings.LowerBody,
            Models.Exercise.MuscleGroupings.Core
        };

        public NewsletterController(CoreContext context)
        {
            _context = context;
        }

        [Route("newsletter")]
        public async Task<IActionResult> Index()
        {
            return await Newsletter(null);
        }

        [Route("newsletter/{email}")]
        public async Task<IActionResult> Newsletter(string? email)
        {
            // TODO: Refactor

            var today = DateOnly.FromDateTime(DateTime.Today);

            User? user = default;
            if (email != null)
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                
                if (user != null && user.NeedsRest)
                {
                    user.NeedsRest = false;
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    return NoContent();
                }

                if (user?.RestDays.HasFlag(RestDaysExtensions.FromDate(today)) == true)
                {
                    return NoContent();
                }
            }

            var todoExerciseType = new ExerciseTypeGroups().First(); // Have to start somewhere
            var previousNewsletter = await _context.Newsletters
                .Where(n => n.User == user)
                .OrderBy(n => n.Date)
                .ThenBy(n => n.Id) // Really just for testing. When two newsletters get sent in the same day, I want a difference exercise set.
                .LastOrDefaultAsync();

            if (previousNewsletter != null)
            {
                todoExerciseType = new ExerciseTypeGroups()
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

            // FIXME: Magic int is magic. But really it's the halfway progression level on the way to exercise mastery.
            var myProgression = user?.Progression ?? 50; 
            var equipment = user?.Equipment ?? Equipment.None;

            var allExercises = (await _context.Exercises
                .Include(e => e.Variations)
                .ThenInclude(v => v.Intensities)
                .Select(e => new {
                    e.Muscles,
                    e.ExerciseType,
                    Variations = e.Variations
                        // Make sure the user owns all the equipment necessary for the exercise
                        .Where(v => equipment.HasFlag(v.Equipment))
                        // Select the current progression of each exercise. Weighted exercises (or resistence) have a null progression
                        .Where(v => (myProgression >= v.Progression && v.Progression > (myProgression - 20)) || v.Progression == null)
                })
                .Where(e => e.Variations.Any())
                .ToListAsync())
                .Select(e => new
                {
                    Exercise = e.Variations
                        // Include both the weighted/resistance (null progression) and calisthenic exercise in the next step
                        .GroupBy(v => v.Progression == null)
                        .Select(g => g.OrderByDescending(v => v.Progression).First())
                        // Select either the weighted/resistance or calisthenics exercise.
                        // We only need one variation per exercise.
                        .OrderBy(v => Guid.NewGuid())
                        .First(),
                    Muscles = e.Muscles,
                    ExerciseType = e.ExerciseType
                })
                // We're choosing random exercises at the moment. That may change later
                .OrderBy(e => Guid.NewGuid());

            var exercises = allExercises
                .Where(e => todoExerciseType.ExerciseType.HasFlag(e.ExerciseType))
                // Make sure the exercise is the correct type and not a warmup exercise
                .Where(e => e.Exercise.Intensities.Any(i => i.IntensityLevel == IntensityLevel.Main))
                .Select(e => new ExerciseViewModel(e.Exercise, IntensityLevel.Main) {
                    ExerciseType = e.ExerciseType,
                    Muscles = e.Muscles 
                })
                .Aggregate(new List<ExerciseViewModel>(), (acc, e) => (
                    // Make sure the exercise covers a unique muscle group
                    !e.Muscles.HasAnyFlag32(acc.Aggregate((MuscleGroups)0, (f, x) => f | x.Muscles))
                    // Make sure the exercise covers some muscle group in the user's least used muscle group history
                    && (!todoExerciseType.MuscleGroups.HasValue || e.Muscles.HasAnyFlag32(todoExerciseType.MuscleGroups.Value))
                ) ? new List<ExerciseViewModel>(acc) { e } : acc);

            var viewModel = new NewsletterViewModel(exercises)
            {
                User = user,
                ExerciseType = todoExerciseType.ExerciseType,
                MuscleGroups = todoExerciseType.MuscleGroups
            };

            if (todoExerciseType.ExerciseType.HasAnyFlag32(ExerciseType.Cardio | ExerciseType.Strength))
            {
                viewModel.WarmupExercises = allExercises
                    // Make sure the exercise is a stretch exercise
                    .Where(e => e.Exercise.Intensities.Any(i => i.IntensityLevel == IntensityLevel.Stretch))
                    .Select(e => new ExerciseViewModel(e.Exercise, IntensityLevel.Stretch)
                    {
                        ExerciseType = e.ExerciseType,
                        Muscles = e.Muscles
                    })
                    .Aggregate(new List<ExerciseViewModel>(), (acc, e) => (
                        // Choose dynamic stretches
                        e.Exercise.MuscleContractions.HasAnyFlag32(MuscleContractions.Concentric | MuscleContractions.Eccentric)
                        // Make sure the exercise covers a unique muscle group
                        && !e.Muscles.HasAnyFlag32(acc.Aggregate((MuscleGroups)0, (f, x) => f | x.Muscles))
                        // Make sure the exercise covers some muscle group in the user's least used muscle group history
                        && (!todoExerciseType.MuscleGroups.HasValue || e.Muscles.HasAnyFlag32(todoExerciseType.MuscleGroups.Value))
                    ) ? new List<ExerciseViewModel>(acc) { e } : acc);

                viewModel.CooldownExercises = allExercises
                    // Make sure the exercise is a stretch exercise
                    .Where(e => e.Exercise.Intensities.Any(i => i.IntensityLevel == IntensityLevel.Stretch))
                    .Select(e => new ExerciseViewModel(e.Exercise, IntensityLevel.Stretch)
                    {
                        ExerciseType = e.ExerciseType,
                        Muscles = e.Muscles
                    })
                    .Aggregate(new List<ExerciseViewModel>(), (acc, e) => (
                        // Choose static stretches
                        e.Exercise.MuscleContractions.HasFlag(MuscleContractions.Isometric)
                        // Make sure the exercise covers a unique muscle group
                        && !e.Muscles.HasAnyFlag32(acc.Aggregate((MuscleGroups)0, (f, x) => f | x.Muscles))
                        // Make sure the exercise covers some muscle group in the user's least used muscle group history
                        && (!todoExerciseType.MuscleGroups.HasValue || e.Muscles.HasAnyFlag32(todoExerciseType.MuscleGroups.Value))
                    ) ? new List<ExerciseViewModel>(acc) { e } : acc);
            }

            return View(nameof(Newsletter), viewModel);
        }
    }
}
