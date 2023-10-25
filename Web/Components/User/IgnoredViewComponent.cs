using Data.Dtos.Newsletter;
using Data.Query.Builders;
using Data.Repos;
using Lib.ViewModels.Newsletter;
using Lib.ViewModels.User;
using Microsoft.AspNetCore.Mvc;
using Web.Code;
using Web.ViewModels.User.Components;

namespace Web.Components.User;


/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class IgnoredViewComponent(IServiceScopeFactory serviceScopeFactory, UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Ignored";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var ignoredExercises = (await new QueryBuilder()
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, ignoreIgnored: true, ignoreMissingEquipment: true, uniqueExercises: false)
            .WithExercises(x =>
            {
                x.AddExercises(user.UserExercises.Where(uv => uv.Ignore).Select(e => e.Exercise));
            })
            .Build()
            .Query(serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r)
            .AsType<ExerciseVariationViewModel, ExerciseVariationDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        var ignoredVariations = new List<ExerciseVariationViewModel>();
        foreach (var section in user.UserVariations.Select(uv => uv.Section).Distinct())
        {
            ignoredVariations.AddRange((await new QueryBuilder(section)
                .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, ignoreIgnored: true, ignoreMissingEquipment: true, uniqueExercises: false)
                .WithExercises(x =>
                {
                    x.AddVariations(user.UserVariations
                        .Where(uv => uv.Ignore)
                        .Where(uv => uv.Section == section)
                        .Select(e => e.Variation)
                    );
                })
                .Build()
                .Query(serviceScopeFactory))
                .Select(r => new ExerciseVariationDto(r)
                .AsType<ExerciseVariationViewModel, ExerciseVariationDto>()!)
                .DistinctBy(vm => vm.Variation)
                .ToList()
            );
        }

        // Need a user context so the manage link is clickable and the user can un-ignore an exercise/variation.
        var userNewsletter = user.AsType<UserNewsletterViewModel, Data.Entities.User.User>()!;
        userNewsletter.Token = await userRepo.AddUserToken(user, durationDays: 1);
        return View("Ignored", new IgnoredViewModel()
        {
            UserNewsletter = userNewsletter,
            IgnoredExercises = ignoredExercises,
            IgnoredVariations = ignoredVariations,
        });
    }
}
