using FinerFettle.Web.Data;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinerFettle.Web.Controllers
{
    [Route("exercises")]
    public class ExerciseController : BaseController
    {
        /// <summary>
        /// The name of the controller for routing purposes
        /// </summary>
        public const string Name = "Exercise";

        public ExerciseController(CoreContext context) : base(context) { }

        [Route("all")]
        public async Task<IActionResult> All()
        {
            // Flatten all exercise variations and intensities into one big list
            var allExercises = (await _context.Variations
                .Include(i => i.Exercise)
                    // To display the exercise prequisite requirements
                    .ThenInclude(e => e.Prerequisites)
                        .ThenInclude(e => e.PrerequisiteExercise)
                // To display the equipment necessary to complete the variation
                .Include(i => i.EquipmentGroups)
                    .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
                .Include(i => i.Intensities)
                .Select(i => new ExerciseViewModel(null, i, null, ExerciseActivityLevel.Main)
                {
                    Verbosity = Verbosity.Diagnostic
                })
                .ToListAsync())
                .OrderBy(vm => vm.Exercise.Name) // OrderBy must come after query or you get duplicates
                .ThenBy(vm => vm.Variation.Progression.Min)
                .ThenBy(vm => vm.Variation.Progression.Max == null)
                .ThenBy(vm => vm.Variation.Progression.Max);

            var exercises = allExercises.Where(vm => vm.ActivityLevel == ExerciseActivityLevel.Main).ToList();
            var viewModel = new ExercisesViewModel(exercises, Verbosity.Diagnostic);

            return View(viewModel);
        }

        [Route("check")]
        public async Task<IActionResult> Check()
        {
            // Flatten all exercise variations and intensities into one big list
            var allExercises = (await _context.Variations
                .Include(i => i.Exercise)
                    .ThenInclude(e => e.Prerequisites)
                        .ThenInclude(e => e.PrerequisiteExercise)
                .Include(i => i.EquipmentGroups)
                    .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
                .Include(i => i.Intensities)
                .Select(i => new ExerciseViewModel(null, i, null, ExerciseActivityLevel.Main)
                {
                    Verbosity = Verbosity.Diagnostic
                })
                .ToListAsync())
                .OrderBy(vm => vm.Exercise.Name) // OrderBy must come after query or you get duplicates
                .ThenBy(vm => vm.Variation.Progression.Min)
                .ThenBy(vm => vm.Variation.Progression.Max == null)
                .ThenBy(vm => vm.Variation.Progression.Max);

            var exercises = allExercises.Where(vm => vm.ActivityLevel == ExerciseActivityLevel.Main).ToList();
            var viewModel = new ExercisesViewModel(exercises, Verbosity.Diagnostic);

            return View(viewModel);
        }
    }
}
