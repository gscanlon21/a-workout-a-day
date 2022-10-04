using FinerFettle.Web.Data;
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
            // Flatten all exercise variations and intensities into one big list
            var allExercises = (await _context.Intensities
                .Include(i => i.Variation)
                    .ThenInclude(v => v.Exercise)
                .Include(i => i.EquipmentGroups)
                    .ThenInclude(eg => eg.Equipment)
                .Include(i => i.IntensityPreferences)
                .Select(i => new ExerciseViewModel(null, i.Variation.Exercise, i.Variation, i)
                {
                    Verbosity = Models.Newsletter.Verbosity.Diagnostic
                })
                .ToListAsync())
                .OrderBy(vm => vm.Exercise.Id) // OrderBy must come after query or you get duplicates
                .ThenBy(vm => vm.Intensity.Progression.Min)
                .ThenBy(vm => vm.Intensity.Progression.Max == null)
                .ThenBy(vm => vm.Intensity.Progression.Max);

            var exercises = allExercises.Where(vm => vm.ActivityLevel == ExerciseActivityLevel.Main).ToList();
            var viewModel = new NewsletterViewModel(exercises, Models.Newsletter.Verbosity.Diagnostic)
            {
                WarmupExercises = allExercises.Where(vm => vm.ActivityLevel == ExerciseActivityLevel.Warmup).ToList(),
                CooldownExercises = allExercises.Where(vm => vm.ActivityLevel == ExerciseActivityLevel.Cooldown).ToList()
            };

            return View(viewModel);
        }
    }
}
