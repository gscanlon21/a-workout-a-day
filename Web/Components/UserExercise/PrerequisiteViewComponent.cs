using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Equipment;
using Core.Models.Newsletter;
using Data;
using Data.Query;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.Prerequisite;
using Web.Views.User;

namespace Web.Components.UserExercise;

/// <summary>
/// Shows prerequisites exercises that have the potential to be seen by the user.
/// </summary>
public class PrerequisiteViewComponent : ViewComponent
{
    private readonly CoreContext _context;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PrerequisiteViewComponent(IServiceScopeFactory serviceScopeFactory, CoreContext context)
    {
        _context = context;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "Prerequisite";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, ManageExerciseVariationViewModel.Params parameters, bool open = false)
    {
        var prerequisites = await _context.ExercisePrerequisites.AsNoTracking()
            .Where(ep => ep.ExerciseId == parameters.ExerciseId)
            .IgnoreQueryFilters().ToListAsync();

        var prerequisiteExercises = (await new QueryBuilder(Section.None)
            // NOTE: Prerequisites hidden by use caution will be skipped.
            // Need a user to grab the UserExercise record.
            .WithUser(user, options =>
            {
                options.IgnoreProgressions = true;
                options.IgnorePrerequisites = true;
            })
            .WithExercises(builder =>
            {
                builder.AddExercisePrerequisites(prerequisites);
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = false;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.ProgressionLevels))
            .Select(r => r.AsType<ExerciseVariationDto>()!)
            .ToList();

        var visiblePrerequisites = new List<ExerciseVariationDto>();
        var invisiblePrerequisites = new List<ExerciseVariationDto>();
        var exerciseProficiencyMap = prerequisites.ToDictionary(p => p.PrerequisiteExerciseId, p => p.Proficiency);
        foreach (var prerequisiteExercise in prerequisiteExercises)
        {
            // Prerequisites that are ignored should not be shown, they will never be seen.
            if (prerequisiteExercise.UserExercise?.Ignore == true)
            {
                continue;
            }

            // The prerequisite is still null when it has not been eligible to be seen yet, the postrequisite is able to be shown in that case.
            // So we don't run into a scenario where a postreq is never encountered if the prereq cannot be encountered.
            if (prerequisiteExercise.UserExercise == null
                // If the prerequisite has become stale, then the exercise will be visible.
                || prerequisiteExercise.UserExercise.LastVisible < DateHelpers.Today.AddDays(-ExerciseConsts.StaleAfterDays)
                // The prerequisite's progression is >= the prerequisite's proficiency level, this exercise can be seen.
                || prerequisiteExercise.UserExercise.Progression >= exerciseProficiencyMap[prerequisiteExercise.Exercise.Id])
            {
                visiblePrerequisites.Add(prerequisiteExercise);
                continue;
            }

            invisiblePrerequisites.Add(prerequisiteExercise);
        }

        // Need a user context so the manage link is clickable and the user can un-ignore an exercise/variation.
        var userNewsletter = new UserNewsletterDto(user.AsType<UserDto>()!, parameters.Token)
        {
            // Show all instructions.
            Equipment = Equipment.All
        };

        return View("Prerequisite", new PrerequisiteViewModel()
        {
            Open = open,
            UserNewsletter = userNewsletter,
            VisiblePrerequisites = visiblePrerequisites,
            InvisiblePrerequisites = invisiblePrerequisites,
            ExerciseProficiencyMap = exerciseProficiencyMap,
        });
    }
}
