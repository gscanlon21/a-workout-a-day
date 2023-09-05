using Data.Dtos.Newsletter;
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

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public IgnoredViewComponent(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var ignoredExercises = (await new QueryBuilder()
            .WithExercises(x =>
            {
                x.AddExercises(user.UserExercises.Where(uv => uv.Ignore).Select(e => e.Exercise));
            })
            .Build()
            .Query(_serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r)
            .AsType<Lib.ViewModels.Newsletter.ExerciseVariationViewModel, ExerciseVariationDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        var ignoredVariations = (await new QueryBuilder()
            .WithExercises(x =>
            {
                x.AddVariations(user.UserVariations.Where(uv => uv.Ignore).Select(e => e.Variation));
            })
            .Build()
            .Query(_serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r)
            .AsType<Lib.ViewModels.Newsletter.ExerciseVariationViewModel, ExerciseVariationDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        return View("Ignored", new IgnoredViewModel()
        {
            IgnoredExercises = ignoredExercises,
            IgnoredVariations = ignoredVariations,
        });
    }
}
