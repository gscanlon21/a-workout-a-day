using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Data;
using Data.Query;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code;
using Web.Views.Shared.Components.IgnoredExerciseVariations;

namespace Web.Components.User;

/// <summary>
/// Displays the user's ignored exercises and variations so they have the ability to unignore them.
/// </summary>
public class IgnoredExerciseVariationsViewComponent : ViewComponent
{
    private readonly CoreContext _context;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public IgnoredExerciseVariationsViewComponent(IServiceScopeFactory serviceScopeFactory, CoreContext context)
    {
        _context = context;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "IgnoredExerciseVariations";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, string token)
    {
        user.UserExercises ??= await _context.UserExercises.Include(uv => uv.Exercise).Where(uv => uv.UserId == user.Id).ToListAsync();
        user.UserVariations ??= await _context.UserVariations.Include(uv => uv.Variation).Where(uv => uv.UserId == user.Id).ToListAsync();

        var ignoredExercises = await new QueryBuilder()
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, ignoreIgnored: true, ignoreMissingEquipment: true, uniqueExercises: false)
            .WithExercises(x =>
            {
                x.AddExercises(user.UserExercises.Where(uv => uv.Ignore));
            })
            .Build()
            .Query(_serviceScopeFactory);

        var ignoredVariations = new List<QueryResults>();
        foreach (var section in user.UserVariations.Select(uv => uv.Section).Distinct())
        {
            ignoredVariations.AddRange((await new QueryBuilder(section)
                .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, ignoreIgnored: true, ignoreMissingEquipment: true, uniqueExercises: false)
                .WithExercises(x =>
                {
                    x.AddVariations(user.UserVariations.Where(uv => uv.Ignore).Where(uv => uv.Section == section));
                })
                .Build()
                .Query(_serviceScopeFactory))
                // Don't show ignored variations when the exercise is also ignored. No information overload.
                .Where(iv => iv.UserExercise?.Ignore != true)
            );
        }

        // Need a user context so the manage link is clickable and the user can un-ignore an exercise/variation.
        var userNewsletter = new UserNewsletterDto(user.AsType<UserDto, Data.Entities.User.User>()!, token);
        return View("IgnoredExerciseVariations", new IgnoredExerciseVariationsViewModel()
        {
            UserNewsletter = userNewsletter,
            IgnoredExercises = ignoredExercises.Select(r => r.AsType<ExerciseVariationDto, QueryResults>()!).ToList(),
            IgnoredVariations = ignoredVariations.Select(r => r.AsType<ExerciseVariationDto, QueryResults>()!).ToList(),
        });
    }
}
