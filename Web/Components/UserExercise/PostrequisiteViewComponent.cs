using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Data;
using Data.Query;
using Data.Query.Builders;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code;
using Web.Views.Shared.Components.Postrequisite;
using Web.Views.User;

namespace Web.Components.UserExercise;

/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class PostrequisiteViewComponent : ViewComponent
{
    private readonly UserRepo _userRepo;
    private readonly CoreContext _context;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PostrequisiteViewComponent(IServiceScopeFactory serviceScopeFactory, CoreContext context, UserRepo userRepo)
    {
        _context = context;
        _userRepo = userRepo;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "Postrequisite";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, ManageExerciseVariationDto.Params parameters)
    {
        var userExercise = await _context.UserExercises.FirstOrDefaultAsync(ue => ue.UserId == user.Id && ue.ExerciseId == parameters.ExerciseId);
        if (userExercise == null)
        {
            return Content("");
        }

        var postrequisites = await _context.ExercisePrerequisites
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
            .Query(_serviceScopeFactory))
            .Select(r => r.AsType<ExerciseVariationDto, QueryResults>()!)
            .ToList();

        // Need a user context so the manage link is clickable and the user can un-ignore an exercise/variation.
        var userNewsletter = user.AsType<UserNewsletterDto, Data.Entities.User.User>()!;
        userNewsletter.Token = await _userRepo.AddUserToken(user, durationDays: 1);
        var viewModel = new PostrequisiteViewModel()
        {
            UserNewsletter = userNewsletter,
            Postrequisites = postrequisites,
            VisiblePostrequisites = [],
            InvisiblePostrequisites = []
        };

        var currentVariations = await _context.UserVariations
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
