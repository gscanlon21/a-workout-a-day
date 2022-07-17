using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.Newsletter;

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
            }

            var todoExerciseType = ExerciseType.Strength; // Have to start somewhere
            var previousNewsletters = await _context.Newsletters
                .Where(n => n.Date > today.AddMonths(-1) && n.Date != today)
                .Where(n => n.User == user)
                .ToListAsync();

            if (previousNewsletters != null && previousNewsletters.Any())
            {
                var yesterdays = previousNewsletters.Last();
                if (yesterdays.ExerciseType == ExerciseType.Aerobic || yesterdays.ExerciseType == ExerciseType.Strength)
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

            // FIXME: Magic int is magic. But really it's the halfway progression level on the way to exercise mastery.
            var minProgression = user?.Progression ?? 50; 
            var equipment = user?.Equipment ?? Equipment.None;
            var exercises = await _context.Exercises
                .Where(e => e.ExerciseType == todoExerciseType)
                .Where(e => equipment.HasFlag(e.Equipment))
                .SelectMany(e => e.Variations)
                .Where(v => v.Progression >= minProgression)
                .ToListAsync();

            // TODO: Muscle groups, and selecting exercises that correspond to a full-body workout

            var newsletter = new Newsletter()
            {
                Date = today,
                User = user,
                ExerciseType = todoExerciseType,
                Equipment = equipment
            };

            _context.Newsletters.Add(newsletter);
            await _context.SaveChangesAsync();

            return View(nameof(Newsletter), new NewsletterViewModel(newsletter, exercises));
        }
    }
}
