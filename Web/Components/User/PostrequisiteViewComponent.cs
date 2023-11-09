using Data.Repos;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Query.Builders;
using Web.ViewModels.Exercise;
using Data.Dtos.Newsletter;
using Lib.ViewModels.Newsletter;
using Web.Code;
using Lib.ViewModels.User;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class PostrequisiteViewComponent(IServiceScopeFactory serviceScopeFactory, CoreContext coreContext, UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Postrequisite";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, Data.Entities.Exercise.Exercise exercise)
    {
        var token = await userRepo.AddUserToken(user, durationDays: 1);
        var prerequisites = await coreContext.ExercisePrerequisites
            .Include(ep => ep.Exercise)
            .Where(ep => ep.PrerequisiteExerciseId == exercise.Id)
            .ToListAsync();

        var prerequisiteExercises = (await new QueryBuilder()
            .WithUser(user, ignoreIgnored: true, ignoreMissingEquipment: true, ignorePrerequisites: true, ignoreProgressions: true, uniqueExercises: false)
            .WithExercises(builder =>
            {
                builder.AddExercises(prerequisites.Select(p => p.Exercise));
            })
            .Build()
            .Query(serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r)
            .AsType<ExerciseVariationViewModel, ExerciseVariationDto>()!)
            .ToList();

        // Need a user context so the manage link is clickable and the user can un-ignore an exercise/variation.
        var userNewsletter = user.AsType<UserNewsletterViewModel, Data.Entities.User.User>()!;
        userNewsletter.Token = await userRepo.AddUserToken(user, durationDays: 1);
        return View("Postrequisite", new PostrequisiteViewModel()
        {
            UserNewsletter = userNewsletter,
            Postrequisites = prerequisiteExercises
        });
    }
}
