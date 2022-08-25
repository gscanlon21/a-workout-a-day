using FinerFettle.Web.Data;
using FinerFettle.Web.Extensions;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinerFettle.Web.Controllers
{
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

        [Route("exercises")]
        public async Task<IActionResult> All()
        {
            // TODO: Refactor

            // Flatten all exercise variations and intensities into one big list
            var allExercises = (await _context.Variations
                .Include(v => v.Exercise)
                .Include(v => v.EquipmentGroups)
                .ThenInclude(e => e.Equipment)
                .Where(v => v.Enabled)
                .SelectMany(v => v.Intensities
                    .Select(i => new {
                        Variation = v,
                        Equipment = v.EquipmentGroups.SelectMany(e => e.Equipment).ToList(),
                        Intensity = i, // Need to select into an anonymous object so Proficiency is included...
                        Muscles = v.Exercise.Muscles,
                        ExerciseType = v.Exercise.ExerciseType
                    }))
                .Select(a => new ExerciseViewModel(null, a.Equipment, a.Variation, a.Intensity, a.Muscles, a.ExerciseType, null))
                .ToListAsync())
                .OrderBy(e => e.Exercise.Exercise.Code)
                .ThenBy(e => e.Intensity.MinProgression)
                .ThenBy(e => e.Intensity.MaxProgression == null)
                .ThenBy(e => e.Intensity.MaxProgression);

            var exercises = allExercises
                // Make sure the exercise is the correct type and not a warmup exercise
                .Where(e => e.ActivityLevel == ExerciseActivityLevel.Main)
                .ToList();

            var viewModel = new NewsletterViewModel(exercises)
            {
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
