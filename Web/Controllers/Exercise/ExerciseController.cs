using Core.Consts;
using Core.Models;
using Core.Models.Exercise;
using Data.Data;
using Data.Data.Query;
using Lib.ViewModels.Equipment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code;
using Web.Code.Attributes;
using Web.ViewModels.Exercise;

namespace Web.Controllers.Exercise;

[Route("exercise")]
public partial class ExerciseController : ViewController
{
    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "Exercise";

    private readonly CoreContext _context;

    public ExerciseController(CoreContext context) : base()
    {
        _context = context;
    }

    [Route("all"), ResponseCompression(Enabled = !DebugConsts.IsDebug)]
    public async Task<IActionResult> All(ExercisesViewModel? viewModel = null)
    {
        viewModel ??= new ExercisesViewModel();
        viewModel.Equipment = (await _context.Equipment
                .Where(e => e.DisabledReason == null)
                .OrderBy(e => e.Name)
                .ToListAsync())
                .AsType<List<EquipmentViewModel>, List<Data.Entities.Equipment.Equipment>>()!;

        var queryBuilder = new QueryBuilder(_context).WithOrderBy(OrderBy.Progression);

        if (viewModel.EquipmentIds != null)
        {
            queryBuilder = queryBuilder.WithEquipment(viewModel.EquipmentIds);
        }

        // FIXME: Only the first WithMuscleGroups filter will apply.
        if (viewModel.StrengthMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleGroups(viewModel.StrengthMuscle.Value, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles;
            });
        }

        if (viewModel.StretchMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleGroups(viewModel.StretchMuscle.Value, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
            });
        }

        if (viewModel.SecondaryMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleGroups(viewModel.SecondaryMuscle.Value, x =>
            {
                x.MuscleTarget = vm => vm.Variation.SecondaryMuscles;
            });
        }

        if (viewModel.Joints.HasValue)
        {
            queryBuilder = queryBuilder.WithJoints(viewModel.Joints.Value);
        }

        if (viewModel.OnlyWeights.HasValue)
        {
            queryBuilder = queryBuilder.WithOnlyWeights(viewModel.OnlyWeights.Value == NoYes.Yes);
        }

        if (viewModel.ExerciseType.HasValue)
        {
            queryBuilder = queryBuilder.WithExerciseType(viewModel.ExerciseType.Value);
        }

        if (viewModel.ExerciseFocus.HasValue)
        {
            queryBuilder = queryBuilder.WithExerciseFocus(viewModel.ExerciseFocus.Value);
        }

        if (viewModel.SportsFocus.HasValue)
        {
            queryBuilder = queryBuilder.WithSportsFocus(viewModel.SportsFocus.Value);
        }

        if (viewModel.MuscleContractions.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleContractions(viewModel.MuscleContractions.Value);
        }

        if (viewModel.MovementPatterns.HasValue)
        {
            queryBuilder = queryBuilder.WithMovementPatterns(viewModel.MovementPatterns.Value);
        }

        if (viewModel.MuscleMovement.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleMovement(viewModel.MuscleMovement.Value);
        }

        var allExercises = (await queryBuilder.Build().Query())
            .Select(r => new Data.Models.Newsletter.ExerciseModel(r.User, r.Exercise, r.Variation, r.ExerciseVariation,
                  r.UserExercise, r.UserExerciseVariation, r.UserVariation,
                  easierVariation: r.EasierVariation, harderVariation: r.HarderVariation,
                  intensityLevel: null, ExerciseTheme.Main)
            {
            }.AsType<Lib.ViewModels.Newsletter.ExerciseViewModel, Data.Models.Newsletter.ExerciseModel>()!)
            .ToList();

        if (viewModel.ShowStaticImages)
        {
            // FIXME: Find a better way.
            allExercises.ForEach(e =>
            {
                e.Variation.AnimatedImage = null;
            });
        }

        viewModel.Exercises = allExercises;

        return View(viewModel);
    }
}
