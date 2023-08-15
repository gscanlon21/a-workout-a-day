using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Web.Code;
using Web.ViewModels.User.Components;

namespace Web.Components.User;


/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class IgnoredViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Ignored";

    private readonly CoreContext _context;

    public IgnoredViewComponent(CoreContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var ignoredExercises = (await new QueryBuilder()
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(MuscleGroups.All)
                .WithoutMuscleTargets(), x =>
                {
                    x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.SecondaryMuscles;
                })
            .WithExercises(x =>
            {
                x.AddExercises(user.UserExercises.Where(uv => uv.Ignore).Select(e => e.Exercise));
            })
            .Build()
            .Query(_context))
            .Select(r => new Data.Dtos.Newsletter.ExerciseDto(r, intensity: null)
            .AsType<Lib.ViewModels.Newsletter.ExerciseViewModel, Data.Dtos.Newsletter.ExerciseDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        var ignoredVariations = (await new QueryBuilder()
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(MuscleGroups.All)
                .WithoutMuscleTargets(), x =>
                {
                    x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.SecondaryMuscles;
                })
            .WithExercises(x =>
            {
                x.AddVariations(user.UserVariations.Where(uv => uv.Ignore).Select(e => e.Variation));
            })
            .Build()
            .Query(_context))
            .Select(r => new Data.Dtos.Newsletter.ExerciseDto(r, intensity: null)
            .AsType<Lib.ViewModels.Newsletter.ExerciseViewModel, Data.Dtos.Newsletter.ExerciseDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        var ignoredExerciseVariations = (await new QueryBuilder()
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(MuscleGroups.All)
                .WithoutMuscleTargets(), x =>
                {
                    x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.SecondaryMuscles;
                })
            .WithExercises(x =>
            {
                x.AddExerciseVariations(user.UserExerciseVariations.Where(uc => uc.Ignore).Select(e => e.ExerciseVariation));
            })
            .Build()
            .Query(_context))
            .Select(r => new Data.Dtos.Newsletter.ExerciseDto(r, intensity: null)
            .AsType<Lib.ViewModels.Newsletter.ExerciseViewModel, Data.Dtos.Newsletter.ExerciseDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        return View("Ignored", new IgnoredViewModel()
        {
            IgnoredExercises = ignoredExercises,
            IgnoredVariations = ignoredVariations,
            IgnoredExerciseVariations = ignoredExerciseVariations,
        });
    }
}
