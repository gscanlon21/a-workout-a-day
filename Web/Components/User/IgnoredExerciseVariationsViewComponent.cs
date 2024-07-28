using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Data.Query;
using Data.Query.Builders;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.Code;
using Web.Views.Shared.Components.IgnoredExerciseVariations;

namespace Web.Components.User;

/// <summary>
/// Displays the user's ignored exercises and variations so they have the ability to unignore them.
/// </summary>
public class IgnoredExerciseVariationsViewComponent(IServiceScopeFactory serviceScopeFactory, UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "IgnoredExerciseVariations";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var ignoredExercises = await new QueryBuilder()
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, ignoreIgnored: true, ignoreMissingEquipment: true, uniqueExercises: false)
            .WithExercises(x =>
            {
                x.AddExercises(user.UserExercises.Where(uv => uv.Ignore).Select(e => e.Exercise));
            })
            .Build()
            .Query(serviceScopeFactory);

        var ignoredVariations = new List<QueryResults>();
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
                // Don't show ignored variations when the exercise is also ignored. No information overload.
                .Where(iv => iv.UserExercise?.Ignore != true)
            );
        }

        // Need a user context so the manage link is clickable and the user can un-ignore an exercise/variation.
        var userNewsletter = user.AsType<UserNewsletterDto, Data.Entities.User.User>()!;
        userNewsletter.Token = await userRepo.AddUserToken(user, durationDays: 1);
        return View("IgnoredExerciseVariations", new IgnoredExerciseVariationsViewModel()
        {
            UserNewsletter = userNewsletter,
            IgnoredExercises = ignoredExercises.Select(r => r.AsType<ExerciseVariationDto, QueryResults>()!).ToList(),
            IgnoredVariations = ignoredVariations.Select(r => r.AsType<ExerciseVariationDto, QueryResults>()!).ToList(),
        });
    }
}
