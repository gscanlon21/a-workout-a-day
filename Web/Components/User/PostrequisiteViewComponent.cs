using Data;
using Data.Dtos.Newsletter;
using Data.Query.Builders;
using Data.Repos;
using Lib.ViewModels.Newsletter;
using Lib.ViewModels.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code;
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
        var userExercise = await coreContext.UserExercises.FirstOrDefaultAsync(ue => ue.UserId == user.Id && ue.ExerciseId == exercise.Id);
        if (userExercise == null)
        {
            return Content("");
        }

        var postrequisites = await coreContext.ExercisePrerequisites
            .Include(ep => ep.Exercise)
            .Where(ep => ep.PrerequisiteExerciseId == exercise.Id)
            .ToListAsync();

        var userExercises = await coreContext.UserExercises
            .Where(ue => ue.UserId == user.Id)
            .Where(ue => postrequisites.Select(p => p.ExerciseId).Contains(ue.ExerciseId))
            .ToListAsync();

        var postrequisiteExercises = (await new QueryBuilder()
            .WithUser(user, ignoreIgnored: true, ignoreMissingEquipment: true, ignorePrerequisites: true, ignoreProgressions: true, uniqueExercises: false)
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
            VisiblePostrequisites = new List<ExerciseVariationViewModel>(),
            InvisiblePostrequisites = new List<ExerciseVariationViewModel>(),
        };

        foreach (var postrequisiteExercise in postrequisiteExercises)
        {
            var postrequisite = postrequisites.First(p => p.ExerciseId == postrequisiteExercise.Exercise.Id);
            var postrequisiteUserExercise = userExercises.FirstOrDefault(ue => ue.ExerciseId == postrequisiteExercise.Exercise.Id);

            // If the postrequisite is ignored, is can't be seen in the workouts.
            if (postrequisiteUserExercise?.Ignore != false)
            {
                viewModel.InvisiblePostrequisites.Add(postrequisiteExercise);
                continue;
            }

            if (userExercise.Progression >= postrequisite.Proficiency || userExercise.Ignore == true)
            {
                viewModel.VisiblePostrequisites.Add(postrequisiteExercise);
                continue;
            }

            viewModel.InvisiblePostrequisites.Add(postrequisiteExercise);
        }

        return View("Postrequisite", viewModel);
    }
}
