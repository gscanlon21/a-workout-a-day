using Data.Dtos.Newsletter;
using Data.Query.Builders;
using Data.Repos;
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
            .AsType<Lib.ViewModels.Newsletter.ExerciseVariationViewModel, ExerciseVariationDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        var ignoredVariations = (await new QueryBuilder()
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, ignoreIgnored: true, ignoreMissingEquipment: true, uniqueExercises: false)
            .WithExercises(x =>
            {
                x.AddVariations(user.UserVariations.Where(uv => uv.Ignore).Select(e => e.Variation));
            })
            .Build()
            .Query(serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r)
            .AsType<Lib.ViewModels.Newsletter.ExerciseVariationViewModel, ExerciseVariationDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

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
