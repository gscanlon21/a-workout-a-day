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
            var query = _context.Exercises
                .SelectMany(e => e.Variations
                .Where(v => v.Enabled).Select(v =>
                new {
                    Variation = v,
                    e.Muscles,
                    e.ExerciseType,
                    Intensities = v.Intensities.Select(i => new { i, i.Proficiency }),
                    EquipmentGroups = v.EquipmentGroups.Select(e => new { e, e.Equipment })
                }));
                 // Select a random subset of exercises

            
            
            var temp = (await query
                .ToListAsync());

            var allExercises = temp
                    // Make sure the user owns all the equipment necessary for the exercise
                    .SelectMany(v => v.Intensities
                        .Select(i => new {
                            Variation = v.Variation,
                            Intensity = i.i, // Need to select into an anonymous object so Proficiency is included...
                            Muscles = v.Muscles,
                            ExerciseType = v.ExerciseType
                        }))
                .Select(a => new ExerciseViewModel(a.Variation, a.Intensity, a.Muscles, a.ExerciseType));

            var exercises = allExercises
                // Make sure the exercise is the correct type and not a warmup exercise
                .Where(e => e.ActivityLevel == ExerciseActivityLevel.Main)
                .ToList();

            var viewModel = new NewsletterViewModel(exercises);

            viewModel.WarmupExercises = allExercises
                // Choose dynamic stretches for warmup
                .Where(e => e.ActivityLevel == ExerciseActivityLevel.Warmup)
                .ToList();

            viewModel.CooldownExercises = allExercises
                // Choose static stretches for cooldown
                .Where(e => e.ActivityLevel == ExerciseActivityLevel.Cooldown)
                .ToList();

            return View("All", viewModel);
        }
    }
}
