using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using FinerFettle.Web.Extensions;

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

        [Route("newsletter")]
        public async Task<IActionResult> Index()
        {
            return await Newsletter(null);
        }

        [Route("newsletter/{email}")]
        public async Task<IActionResult> Newsletter(string? email)
        {
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
                if (user?.NeedsRest == true || yesterdays.ExerciseType == ExerciseType.Aerobic || yesterdays.ExerciseType == ExerciseType.Strength)
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

            todoExerciseType = ExerciseType.Strength; // REMOVEME: Testing

            // FIXME: Magic int is magic. But really it's the halfway progression level on the way to exercise mastery.
            var myProgression = user?.Progression ?? 50; 
            var equipment = user?.Equipment ?? Equipment.None;

            var strengthExercises = (await _context.Exercises
                .Where(e => e.ExerciseType == todoExerciseType)
                .Select(e => new { e.Variations, e.Muscles })
                .ToListAsync())
                .Select(e => new ExerciseViewModel() {
                    Exercise = e.Variations
                        // Make sure the user owns all the equipment necessary for the exercise
                        .Where(v => equipment.HasFlag(v.Equipment))
                        // Select the current progression of each exercise. Weighted exercises (or resistence) have a null progression
                        .Where(v => myProgression >= v.Progression || v.Progression == null)
                        .GroupBy(v => v.Progression == null)
                        .Select(g => g.OrderByDescending(v => v.Progression).FirstOrDefault())
                        .OrderBy(v => Guid.NewGuid())
                        .FirstOrDefault(), 
                    Muscles = e.Muscles })
                .Where(e => e.Exercise != null)
                // We're choosing random exercises at the moment. That may change later
                .OrderBy(e => Guid.NewGuid())
                // Grab exercises that have atleast one muscle not worked out by any other exercise in the workout
                .Aggregate(new List<ExerciseViewModel>(), (acc, e) => e.Muscles.GetFlags().Any(
                    m => !acc.Aggregate((MuscleGroups)0, (f, x) => f | x.Muscles).HasFlag(m)
                ) ? new List<ExerciseViewModel>(acc) { e } : acc)
                .ToList();

            return View(nameof(Newsletter), new NewsletterViewModel(strengthExercises)
            {
                User = user
            });
        }
    }
}
