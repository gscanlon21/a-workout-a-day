using FinerFettle.Web.Data;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.ViewModels.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NuGet.Common;

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
            var allExercises = new ExerciseQueryBuilder(_context, user: null)
                .WithMuscleGroups(MuscleGroups.All)
                .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.Progression)
                .Query()
                .Select(r => new ExerciseViewModel(r, ExerciseActivityLevel.Main))
                .ToList();
         
            var viewModel = new ExercisesViewModel(Verbosity.Debug, allExercises);

            return View(viewModel);
        }

        [Route("check")]
        public async Task<IActionResult> Check()
        {
            var allExercises = new ExerciseQueryBuilder(_context, user: null)
                .WithMuscleGroups(MuscleGroups.All)
                .Query()
                .Select(r => new ExerciseViewModel(r, ExerciseActivityLevel.Main))
                .ToList();

            var recoveryExercises = new ExerciseQueryBuilder(_context, user: null)
                .WithMuscleGroups(MuscleGroups.All)
                .WithRecoveryMuscle(MuscleGroups.All, include: true)
                .Query()
                .Select(r => new ExerciseViewModel(r, ExerciseActivityLevel.Main))
                .ToList();

            var warmupCooldownExercises = new ExerciseQueryBuilder(_context, user: null)
                .WithMuscleGroups(MuscleGroups.All)
                .WithExerciseType(ExerciseType.Flexibility | ExerciseType.Cardio)
                .Query()
                .Select(r => new ExerciseViewModel(r, ExerciseActivityLevel.Main))
                .ToList();

            var missing100PProgressionRange = allExercises.GroupBy(e => e.Exercise.Name)
                .Where(e =>
                    e.Min(i => i.ExerciseProgression.Progression.GetMinOrDefault) > 0
                || e.Max(i => i.ExerciseProgression.Progression.GetMaxOrDefault) < 100)
                .Select(e => e.Key)
                .ToList();

            var missingRepRange = allExercises
                .Where(e => e.Variation.Intensities.Any(p => (p.Proficiency.MinReps != null && p.Proficiency.MaxReps == null) || (p.Proficiency.MinReps == null && p.Proficiency.MaxReps != null)))
                .Select(e => e.Variation.Name)
                .ToList();

            var missingProficiency = new List<string>();
            missingProficiency.AddRange(allExercises
                .Where(e => e.Variation.Intensities.All(p =>
                    p.IntensityLevel != IntensityLevel.Maintain
                    && p.IntensityLevel != IntensityLevel.Obtain
                    && p.IntensityLevel != IntensityLevel.Gain
                    && p.IntensityLevel != IntensityLevel.Endurance
                ))
                .Select(e => e.Variation.Name));
            missingProficiency.AddRange(recoveryExercises
                .Where(e => e.Variation.Intensities.All(p =>
                    p.IntensityLevel != IntensityLevel.Recovery
                ))
                .Select(e => e.Variation.Name));
            missingProficiency.AddRange(warmupCooldownExercises
                .Where(e => e.Variation.Intensities.All(p =>
                    p.IntensityLevel != IntensityLevel.WarmupCooldown
                ))
                .Select(e => e.Variation.Name));

            var viewModel = new CheckViewModel()
            {
                Missing100PProgressionRange = missing100PProgressionRange,
                MissingProficiency = missingProficiency,
                MissingRepRange = missingRepRange
            };

            return View(viewModel);
        }
    }
}
