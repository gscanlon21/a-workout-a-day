using Core.Consts;
using Core.Models.Newsletter;
using Data.Dtos.Newsletter;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Web.Code;
using Web.Code.Attributes;
using Web.ViewModels.Exercise;

namespace Web.Controllers.Exercise;

[Route("exercise")]
public partial class ExerciseController(IServiceScopeFactory serviceScopeFactory) : ViewController()
{
    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "Exercise";

    [Route("all"), ResponseCompression(Enabled = !DebugConsts.IsDebug)]
    public async Task<IActionResult> All(ExercisesViewModel? viewModel = null)
    {
        viewModel ??= new ExercisesViewModel();

        var queryBuilder = new QueryBuilder(viewModel.Section ?? Section.None);

        if (viewModel.Equipment.HasValue)
        {
            queryBuilder = queryBuilder.WithEquipment(viewModel.Equipment.Value);
        }

        // FIXME: Only the first WithMuscleGroups filter will apply.
        if (viewModel.StrengthMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups([viewModel.StrengthMuscle.Value])
                .WithoutMuscleTargets(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles;
            });
        }

        if (viewModel.StretchMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups([viewModel.StretchMuscle.Value])
                .WithoutMuscleTargets(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
            });
        }

        if (viewModel.SecondaryMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups([viewModel.SecondaryMuscle.Value])
                .WithoutMuscleTargets(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.SecondaryMuscles;
            });
        }

        if (viewModel.Joints.HasValue)
        {
            queryBuilder = queryBuilder.WithJoints(viewModel.Joints.Value);
        }

        if (viewModel.ExerciseFocus.HasValue)
        {
            queryBuilder = queryBuilder.WithExerciseFocus([viewModel.ExerciseFocus.Value]);
        }

        if (viewModel.SportsFocus.HasValue)
        {
            queryBuilder = queryBuilder.WithSportsFocus(viewModel.SportsFocus.Value);
        }

        if (viewModel.MovementPatterns.HasValue)
        {
            queryBuilder = queryBuilder.WithMovementPatterns(viewModel.MovementPatterns.Value);
        }

        if (viewModel.MuscleMovement.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleMovement(viewModel.MuscleMovement.Value);
        }

        viewModel.Exercises = (await queryBuilder.Build().Query(serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r)
            .AsType<Lib.ViewModels.Newsletter.ExerciseVariationViewModel, ExerciseVariationDto>()!)
            .ToList();

        if (!string.IsNullOrWhiteSpace(viewModel.Name))
        {
            viewModel.Exercises = viewModel.Exercises.Where(e =>
                e.Variation.Name.Contains(viewModel.Name, StringComparison.OrdinalIgnoreCase)
                || e.Exercise.Name.Contains(viewModel.Name, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        return View(viewModel);
    }
}
