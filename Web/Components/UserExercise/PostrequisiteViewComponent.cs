using Data;
using Data.Dtos.Newsletter;
using Data.Query.Builders;
using Data.Repos;
using Lib.ViewModels.Newsletter;
using Lib.ViewModels.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code;
using Web.ViewModels.Components.UserExercise;
using Web.ViewModels.User;

namespace Web.Components.UserExercise;

/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class PostrequisiteViewComponent(IServiceScopeFactory serviceScopeFactory, CoreContext coreContext, UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Postrequisite";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, ManageExerciseVariationViewModel.Params parameters)
    {
        var token = await userRepo.AddUserToken(user, durationDays: 1);
        var userExercise = await coreContext.UserExercises.FirstOrDefaultAsync(ue => ue.UserId == user.Id && ue.ExerciseId == parameters.ExerciseId);
        if (userExercise == null)
        {
            return Content("");
        }

        var postrequisites = await coreContext.ExercisePrerequisites
            .Include(ep => ep.Exercise)
            .Where(ep => ep.PrerequisiteExerciseId == parameters.ExerciseId)
            .ToListAsync();

        var postrequisiteExercises = (await new QueryBuilder()
            .WithUser(user, ignorePrerequisites: true, ignoreProgressions: true, uniqueExercises: false)
            .WithExercises(builder =>
            {
                builder.AddExercises(postrequisites.Select(p => p.Exercise));
            })
            .Build()
            .Query(serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r)
            .AsType<ExerciseVariationViewModel, ExerciseVariationDto>()!)
            .ToList();

        // Need a user context so the manage link is clickable and the user can un-ignore an exercise/variation.
        var userNewsletter = user.AsType<UserNewsletterViewModel, Data.Entities.User.User>()!;
        userNewsletter.Token = await userRepo.AddUserToken(user, durationDays: 1);
        var viewModel = new PostrequisiteViewModel()
        {
            UserNewsletter = userNewsletter,
            Postrequisites = postrequisites,
            VisiblePostrequisites = [],
            InvisiblePostrequisites = []
        };

        var currentVariations = await coreContext.UserVariations
            .Where(uv => uv.UserId == user.Id)
            .Where(uv => uv.Variation.ExerciseId == parameters.ExerciseId)
            .ToListAsync();

        foreach (var postrequisiteExercise in postrequisiteExercises)
        {
            var postrequisite = postrequisites.First(p => p.ExerciseId == postrequisiteExercise.Exercise.Id);

            // The exercise's progression is >= the postrequisite's proficiency level, this exercise can be seen.
            if (userExercise.Progression >= postrequisite.Proficiency
                // If the current exercise is ignored, then the postrequisite will be visible.                
                || userExercise.Ignore == true
                // If all current variations are ignored, then the postrequisite will be visible.
                || currentVariations.All(v => v.Ignore))
            {
                viewModel.VisiblePostrequisites.Add(postrequisiteExercise);
                continue;
            }

            viewModel.InvisiblePostrequisites.Add(postrequisiteExercise);
        }

        return View("Postrequisite", viewModel);
    }
}
