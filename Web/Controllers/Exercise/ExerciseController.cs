using Core.Dtos.Newsletter;
using Core.Models.Exercise.Skills;
using Core.Models.Newsletter;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Web.Code.Attributes;
using Web.Views.Exercise;

namespace Web.Controllers.Exercise;

[Route("exercise", Order = 1)]
[Route("exercises", Order = 2)]
public class ExerciseController : ViewController
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ExerciseController(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// The name of the controller for routing purposes.
    /// </summary>
    public const string Name = "Exercise";

    [Route("", Order = 2)]
    [Route("all", Order = 1)]
    [ResponseCompression(Enabled = !DebugConsts.IsDebug)]
    public async Task<IActionResult> All(ExercisesViewModel? viewModel = null)
    {
        viewModel ??= new ExercisesViewModel();
        if (!ModelState.IsValid)
        {
            viewModel.FormOpen = true;
            return View(viewModel);
        }

        var queryBuilder = new QueryBuilder(viewModel.Section ?? Section.None);

        if (viewModel.Equipment.HasValue)
        {
            queryBuilder = queryBuilder.WithEquipment(viewModel.Equipment.Value);
        }

        if (viewModel.StrengthMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups([viewModel.StrengthMuscle.Value])
                .WithoutMuscleTargets(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.Strengthens;
            });
        }

        if (viewModel.StretchMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups([viewModel.StretchMuscle.Value])
                .WithoutMuscleTargets(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.Stretches;
            });
        }

        if (viewModel.SecondaryMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups([viewModel.SecondaryMuscle.Value])
                .WithoutMuscleTargets(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.Stabilizes;
            });
        }

        if (viewModel.VisualSkills.HasValue)
        {
            queryBuilder = queryBuilder.WithSkills(SkillTypes.VisualSkills, viewModel.VisualSkills, options =>
            {
                options.RequireSkills = true;
            });
        }

        if (viewModel.CervicalSkills.HasValue)
        {
            queryBuilder = queryBuilder.WithSkills(SkillTypes.CervicalSkills, viewModel.CervicalSkills, options =>
            {
                options.RequireSkills = true;
            });
        }

        if (viewModel.ThoracicSkills.HasValue)
        {
            queryBuilder = queryBuilder.WithSkills(SkillTypes.ThoracicSkills, viewModel.ThoracicSkills, options =>
            {
                options.RequireSkills = true;
            });
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

        viewModel.Exercises = (await queryBuilder.Build().Query(_serviceScopeFactory))
            .Select(r => r.AsType<ExerciseVariationDto>()!)
            .ToList();

        if (!string.IsNullOrWhiteSpace(viewModel.Name))
        {
            viewModel.Exercises = viewModel.Exercises.Where(e =>
                e.Exercise.Name.Contains(viewModel.Name, StringComparison.OrdinalIgnoreCase)
                || e.Variation.Name.Contains(viewModel.Name, StringComparison.OrdinalIgnoreCase)
                || e.Exercise.Notes?.Contains(viewModel.Name, StringComparison.OrdinalIgnoreCase) == true
                || e.Variation.Notes?.Contains(viewModel.Name, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();
        }

        return View(viewModel);
    }
}
