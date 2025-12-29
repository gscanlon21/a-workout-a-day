using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Equipment;
using Core.Models.Newsletter;
using Data;
using Data.Entities.Users;
using Data.Query;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.Postrequisite;
using Web.Views.User;

namespace Web.Components.UserExercise;

/// <summary>
/// Shows postrequisite exercises that have the potential to be seen by the user.
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

    public async Task<IViewComponentResult> InvokeAsync(User user, ManageExerciseVariationViewModel.Params parameters, bool open = false)
    {
        var userExercise = await _context.UserExercises.FirstOrDefaultAsync(ue => ue.UserId == user.Id && ue.ExerciseId == parameters.ExerciseId);
        if (userExercise == null)
        {
            return Content("");
        }

        var postrequisites = await _context.ExercisePrerequisites.AsNoTracking()
            .Where(ep => ep.PrerequisiteExerciseId == parameters.ExerciseId)
            .IgnoreQueryFilters().ToListAsync();

        var postrequisiteExercises = (await new UserQueryBuilder(user, Section.None)
            // NOTE: Prostrequisites hidden by use caution will be skipped.
            // Need a user to grab the UserExercise record.
            .WithUser(options =>
            {
                options.IgnoreProgressions = true;
                options.IgnorePrerequisites = true;
            })
            .WithExercises(builder =>
            {
                builder.AddExercisePostrequisites(postrequisites);
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = false;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.ProgressionLevels))
            .Select(r => r.AsType<ExerciseVariationDto>()!)
            .ToList();

        var visiblePostrequisites = new List<ExerciseVariationDto>();
        var invisiblePostrequisites = new List<ExerciseVariationDto>();
        var exerciseProficiencyMap = postrequisites.ToDictionary(p => p.ExerciseId, p => p.Proficiency);
        foreach (var postrequisiteExercise in postrequisiteExercises)
        {
            // Postrequisites that are ignored should not be shown, they will never be seen.
            if (postrequisiteExercise.UserExercise?.Ignore == true)
            {
                continue;
            }

            // The exercise's progression is >= the postrequisite's proficiency level, this postrequisite can be seen.
            if (userExercise.Progression >= exerciseProficiencyMap[postrequisiteExercise.Exercise.Id]
                // If the exercise has become stale, then the postrequisite will be visible.
                || userExercise.LastVisible < DateHelpers.Today.AddDays(-ExerciseConsts.StaleAfterDays)
                // If the exercise is ignored, then the postrequisite will be visible.                
                || userExercise.Ignore == true)
            {
                visiblePostrequisites.Add(postrequisiteExercise);
                continue;
            }

            invisiblePostrequisites.Add(postrequisiteExercise);
        }

        // Need a user context so the manage link is clickable and the user can un-ignore an exercise/variation.
        var userNewsletter = new UserNewsletterDto(user.AsType<UserDto>()!, parameters.Token)
        {
            // Show all instructions.
            Equipment = Equipment.All
        };

        return View("Postrequisite", new PostrequisiteViewModel()
        {
            Open = open,
            UserNewsletter = userNewsletter,
            VisiblePostrequisites = visiblePostrequisites,
            InvisiblePostrequisites = invisiblePostrequisites,
            ExerciseProficiencyMap = exerciseProficiencyMap,
        });
    }
}
