using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Data;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.Postrequisite;
using Web.Views.User;

namespace Web.Components.UserExercise;

/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class PostrequisiteViewComponent : ViewComponent
{
    private readonly CoreContext _context;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PostrequisiteViewComponent(IServiceScopeFactory serviceScopeFactory, CoreContext context)
    {
        _context = context;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "Postrequisite";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, ManageExerciseVariationViewModel.Params parameters)
    {
        var userExercise = await _context.UserExercises.FirstOrDefaultAsync(ue => ue.UserId == user.Id && ue.ExerciseId == parameters.ExerciseId);
        if (userExercise == null)
        {
            return Content("");
        }

        var postrequisites = await _context.ExercisePrerequisites.AsNoTracking()
            .Where(ep => ep.PrerequisiteExerciseId == parameters.ExerciseId)
            .IgnoreQueryFilters().ToListAsync();

        var postrequisiteExercises = (await new QueryBuilder()
            .WithExercises(builder =>
            {
                builder.AddExercisePostrequisites(postrequisites);
            })
            .Build()
            .Query(_serviceScopeFactory))
            .Select(r => r.AsType<ExerciseVariationDto>()!)
            .ToList();

        // Need a user context so the manage link is clickable and the user can un-ignore an exercise/variation.
        var userNewsletter = new UserNewsletterDto(user.AsType<UserDto>()!, parameters.Token);
        var viewModel = new PostrequisiteViewModel()
        {
            VisiblePostrequisites = [],
            InvisiblePostrequisites = [],
            UserNewsletter = userNewsletter,
            ExerciseProficiencyMap = postrequisites.ToDictionary(p => p.ExerciseId, p => p.Proficiency),
        };

        var currentVariations = await _context.UserVariations
            .Where(uv => uv.Variation.ExerciseId == parameters.ExerciseId)
            .Where(uv => uv.UserId == user.Id)
            .ToListAsync();

        foreach (var postrequisiteExercise in postrequisiteExercises)
        {
            // The exercise's progression is >= the postrequisite's proficiency level, this exercise can be seen.
            if (userExercise.Progression >= viewModel.ExerciseProficiencyMap[postrequisiteExercise.Exercise.Id]
                // If all current variations are ignored, then the postrequisite will be visible.
                || currentVariations.All(v => v.Ignore)
                // If the current exercise is ignored, then the postrequisite will be visible.                
                || userExercise.Ignore == true)
            {
                viewModel.VisiblePostrequisites.Add(postrequisiteExercise);
                continue;
            }

            viewModel.InvisiblePostrequisites.Add(postrequisiteExercise);
        }

        return View("Postrequisite", viewModel);
    }
}
