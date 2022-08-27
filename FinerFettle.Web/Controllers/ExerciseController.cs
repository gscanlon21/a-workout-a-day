using FinerFettle.Web.Data;
using FinerFettle.Web.Extensions;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinerFettle.Web.Controllers
{
    [Route("exercises")]
    public class ExerciseController : Controller
    {
        private readonly CoreContext _context;

        /// <summary>
        /// The name of the controller for routing purposes
        /// </summary>
        public const string Name = "Exercise";

        public ExerciseController(CoreContext context)
        {
            _context = context;
        }

        [Route("all")]
        public async Task<IActionResult> All()
        {
            // TODO: Refactor

            // Flatten all exercise variations and intensities into one big list
            var allExercises = (await _context.Variations
                .Include(v => v.Exercise)
                .Include(v => v.Intensities)
                .ThenInclude(i => i.EquipmentGroups)
                .ThenInclude(e => e.Equipment)
                .Where(v => v.DisabledReason == null)
                .SelectMany(v => v.Intensities
                    .Select(i => new {
                        Variation = v,
                        Intensity = i, // Need to select into an anonymous object so Proficiency is included...
                    }))
                .Select(a => new ExerciseViewModel(null, a.Variation.Exercise, a.Variation, a.Intensity))
                .ToListAsync())
                .OrderBy(e => e.Exercise.Id)
                .ThenBy(e => e.Intensity.Progression.Min)
                .ThenBy(e => e.Intensity.Progression.Max == null)
                .ThenBy(e => e.Intensity.Progression.Max);

            var exercises = allExercises
                // Make sure the exercise is the correct type and not a warmup exercise
                .Where(e => e.ActivityLevel == ExerciseActivityLevel.Main)
                .ToList();

            var viewModel = new NewsletterViewModel(exercises)
            {
                Verbose = true,
                WarmupExercises = allExercises
                    // Choose dynamic stretches for warmup
                    .Where(e => e.ActivityLevel == ExerciseActivityLevel.Warmup)
                    .ToList(),
                CooldownExercises = allExercises
                    // Choose static stretches for cooldown
                    .Where(e => e.ActivityLevel == ExerciseActivityLevel.Cooldown)
                    .ToList()
            };

            return View(viewModel);
        }
    }
}
