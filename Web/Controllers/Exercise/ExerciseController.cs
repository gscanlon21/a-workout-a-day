using Web.Code.Attributes.Response;
using Web.Data;
using Web.Models.Exercise;
using Web.ViewModels.Exercise;
using Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data.Query;

namespace Web.Controllers.Exercise;

[Route("exercise")]
public partial class ExerciseController : BaseController
{
    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "Exercise";

    public ExerciseController(CoreContext context) : base(context) { }

    [Route("all"), EnableRouteResponseCompression]
    public async Task<IActionResult> All(ExercisesViewModel? viewModel = null)
    {
        viewModel ??= new ExercisesViewModel();
        viewModel.Equipment = await _context.Equipment
                .Where(e => e.DisabledReason == null)
                .OrderBy(e => e.Name)
                .ToListAsync();

        var queryBuilder = new QueryBuilder(_context)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles;
            })
            .WithOrderBy(OrderBy.Progression);

        if (viewModel.SportsFocus.HasValue)
        {
            queryBuilder = queryBuilder.WithSportsFocus(viewModel.SportsFocus.Value);
        }

        if (viewModel.RecoveryMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithRecoveryMuscle(viewModel.RecoveryMuscle.Value);
        }

        if (!viewModel.ShowFilteredOut)
        {
            if (viewModel.EquipmentIds != null)
            {
                queryBuilder = queryBuilder.WithEquipment(viewModel.EquipmentIds);
            }

            if (viewModel.OnlyUnilateral.HasValue)
            {
                queryBuilder = queryBuilder.IsUnilateral(viewModel.OnlyUnilateral == Models.NoYes.Yes);
            }

            if (viewModel.IncludeMuscle.HasValue)
            {
                queryBuilder = queryBuilder.WithMuscleGroups(viewModel.IncludeMuscle.Value, x =>
                {
                    x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles;
                });
            }

            if (viewModel.OnlyWeights.HasValue)
            {
                queryBuilder = queryBuilder.WithOnlyWeights(viewModel.OnlyWeights.Value == Models.NoYes.Yes);
            }

            if (viewModel.OnlyAntiGravity.HasValue)
            {
                queryBuilder = queryBuilder.WithAntiGravity(viewModel.OnlyAntiGravity.Value == Models.NoYes.Yes);
            }

            if (viewModel.ExerciseType.HasValue)
            {
                queryBuilder = queryBuilder.WithExerciseType(viewModel.ExerciseType.Value);
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
        }

        var allExercises = (await queryBuilder.Build().Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        if (viewModel.ShowFilteredOut)
        {
            if (viewModel.OnlyUnilateral.HasValue)
            {
                var temp = Filters.FilterIsUnilateral(allExercises.AsQueryable(), viewModel.OnlyUnilateral.Value == Models.NoYes.Yes);
                allExercises.ForEach(e =>
                {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.IncludeMuscle.HasValue)
            {
                var temp = Filters.FilterMuscleGroup(allExercises.AsQueryable(), viewModel.IncludeMuscle, include: true, muscleTarget: vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles);
                allExercises.ForEach(e =>
                {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.EquipmentIds != null)
            {
                var temp = Filters.FilterEquipmentIds(allExercises.AsQueryable(), viewModel.EquipmentIds);
                allExercises.ForEach(e =>
                {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.OnlyWeights.HasValue)
            {
                var temp = Filters.FilterOnlyWeights(allExercises.AsQueryable(), viewModel.OnlyWeights.Value == Models.NoYes.Yes);
                allExercises.ForEach(e =>
                {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.OnlyAntiGravity.HasValue)
            {
                var temp = Filters.FilterAntiGravity(allExercises.AsQueryable(), viewModel.OnlyAntiGravity.Value == Models.NoYes.Yes);
                allExercises.ForEach(e =>
                {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.ExerciseType.HasValue)
            {
                var temp = Filters.FilterExerciseType(allExercises.AsQueryable(), viewModel.ExerciseType);
                allExercises.ForEach(e =>
                {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.MovementPatterns.HasValue)
            {
                var temp = Filters.FilterMovementPattern(allExercises.AsQueryable(), viewModel.MovementPatterns);
                allExercises.ForEach(e =>
                {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.MuscleContractions.HasValue)
            {
                var temp = Filters.FilterMuscleContractions(allExercises.AsQueryable(), viewModel.MuscleContractions);
                allExercises.ForEach(e =>
                {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.MuscleMovement.HasValue)
            {
                var temp = Filters.FilterMuscleMovement(allExercises.AsQueryable(), viewModel.MuscleMovement);
                allExercises.ForEach(e =>
                {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }
        }

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
