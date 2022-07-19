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
                    var user2 = user;
                    user2.NeedsRest = false;
                    _context.Update(user2);
                    await _context.SaveChangesAsync();
                }
            }

            var todoExerciseType = ExerciseType.Strength; // Have to start somewhere
            var previousNewsletters = await _context.Newsletters
                .Where(n => n.Date > today.AddMonths(-1) && n.Date != today)
                .Where(n => n.User == user)
                .ToListAsync();

            if (previousNewsletters != null && previousNewsletters.Any())
            {
                var yesterdays = previousNewsletters.Last();
                if (user?.NeedsRest == true || yesterdays.ExerciseType.HasFlag(ExerciseType.Aerobic))
                {
                    // Yesterday was tough. Enjoy a break.
                    todoExerciseType = ExerciseType.None;
                }
                else
                {
                    todoExerciseType = previousNewsletters
                        .GroupBy(n => n.ExerciseType)
                        .Select(t => new { t.Key, Count = t.Count() })
                        .Where(g => g.Key != ExerciseType.None)
                        ?.MinBy(g => g.Count)?.Key ?? todoExerciseType;
                }
            }

            todoExerciseType = ExerciseTypeGroups.StretchStrength; // REMOVEME: Testing

            // FIXME: Magic int is magic. But really it's the halfway progression level on the way to exercise mastery.
            var myProgression = user?.Progression ?? 50; 
            var equipment = user?.Equipment ?? Equipment.None;

            IEnumerable<ExerciseViewModel> eligibleExercises = (await _context.Exercises
               .Where(e => todoExerciseType.HasFlag(e.ExerciseType))
               .Select(e => new { e.Variations, e.Muscles, e.ExerciseType })
               .ToListAsync())
               .Select(e => new ExerciseViewModel()
               {
                   Exercise = e.Variations
                       // Make sure the user owns all the equipment necessary for the exercise
                       .Where(v => equipment.HasFlag(v.Equipment))
                       // Select the current progression of each exercise. Weighted exercises (or resistence) have a null progression
                       .Where(v => (myProgression >= v.Progression && v.Progression > (myProgression - 20)) || v.Progression == null)
                       // Include both the weighted/resistance (null progression) and calisthenic exercise in the next step
                       .GroupBy(v => v.Progression == null)
                       .Select(g => g.OrderByDescending(v => v.Progression).FirstOrDefault())
                       // Select either the weighted/resistance or calisthenics exercise.
                       // We only need one variation per exercise.
                       .OrderBy(v => Guid.NewGuid())
                       .FirstOrDefault(),
                   Muscles = e.Muscles,
                   ExerciseType = e.ExerciseType
               })
               .Where(e => e.Exercise != null)
               // We're choosing random exercises at the moment. That may change later
               .OrderBy(e => Guid.NewGuid());

            var viewModel = new NewsletterViewModel(user)
            {
                ExerciseType = todoExerciseType
            };

            if (todoExerciseType.HasFlag(ExerciseType.Strength))
            {
                var musclesWorkedOverThePathMonth = await _context.Newsletters
                    .Where(n => n.User == user)
                    .Select(n => n.MuscleGroups)
                    .ToListAsync();

                viewModel.MuscleGroups = MuscleGroupings
                    .OrderBy(g => musclesWorkedOverThePathMonth.Sum(m => g.HasFlag(m) ? 1 : 0))
                    .First();

                if (todoExerciseType.HasFlag(ExerciseType.Stretch))
                {
                    viewModel.WarmupExercises = eligibleExercises.Aggregate(new List<ExerciseViewModel>(), (acc, e) =>
                        // Make sure the exercise is a stretch exercise
                        (e.ExerciseType.HasFlag(ExerciseType.Stretch)
                        // Make sure the exercise covers a unique muscle group
                        && e.Muscles.GetFlags().Any(
                            m => !acc.Aggregate((MuscleGroups)0, (f, x) => f | x.Muscles).HasFlag(m))
                        // Make sure the exercise covers some muscle group in the user's least used muscle group history
                        && e.Muscles.GetFlags().Any(
                            m => viewModel.MuscleGroups.Value.HasFlag(m))
                    ) ? new List<ExerciseViewModel>(acc) { e } : acc);
                }

                viewModel.Exercises = eligibleExercises.Aggregate(new List<ExerciseViewModel>(), (acc, e) =>
                    // Make sure the exercise is a stretch exercise
                    (e.ExerciseType.HasFlag(ExerciseType.Strength)
                    // Make sure the exercise covers a unique muscle group
                    && e.Muscles.GetFlags().Any(
                        m => !acc.Aggregate((MuscleGroups)0, (f, x) => f | x.Muscles).HasFlag(m))
                    // Make sure the exercise covers some muscle group in the user's least used muscle group history
                    && e.Muscles.GetFlags().Any(
                        m => viewModel.MuscleGroups.Value.HasFlag(m))
                ) ? new List<ExerciseViewModel>(acc) { e } : acc);
            }

            var newsletter = new Newsletter()
            {
                Date = today,
                User = user,
                ExerciseType = todoExerciseType,
                MuscleGroups = viewModel.MuscleGroups
            };

            _context.Newsletters.Add(newsletter);
            await _context.SaveChangesAsync();

            viewModel.ExerciseType &= ~ExerciseType.Stretch;
            return View(nameof(Newsletter), viewModel);
        }
    }
}
