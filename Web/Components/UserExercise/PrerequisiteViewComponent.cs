using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Data;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.Prerequisite;
using Web.Views.User;

namespace Web.Components.UserExercise;

/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
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

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, ManageExerciseVariationViewModel.Params parameters)
    {
        var prerequisites = await _context.ExercisePrerequisites.AsNoTracking()
            .Where(ep => ep.ExerciseId == parameters.ExerciseId)
            .IgnoreQueryFilters().ToListAsync();

        // Variations missing equipment or that are ignored are not shown, since they will never be seen.
        var prerequisiteExercises = (await new QueryBuilder()
            .WithExercises(builder =>
            {
                builder.AddExercisePrerequisites(prerequisites);
            })
            .Build()
            .Query(_serviceScopeFactory))
            .Select(r => r.AsType<ExerciseVariationDto>()!)
            .ToList();

        // Need a user context so the manage link is clickable and the user can un-ignore an exercise/variation.
        var userNewsletter = new UserNewsletterDto(user.AsType<UserDto>()!, parameters.Token);
        var viewModel = new PrerequisiteViewModel()
        {
            VisiblePrerequisites = [],
            InvisiblePrerequisites = [],
            UserNewsletter = userNewsletter,
            ExerciseProficiencyMap = prerequisites.ToDictionary(p => p.PrerequisiteExerciseId, p => p.Proficiency),
        };

        var userExercises = await _context.UserExercises.Where(ue => ue.UserId == user.Id).AsNoTracking()
            .Where(ue => prerequisites.Select(p => p.PrerequisiteExerciseId).Contains(ue.ExerciseId))
            .IgnoreQueryFilters().ToListAsync();

        foreach (var prerequisiteExercise in prerequisiteExercises)
        {
            var prerequisiteUserExercise = userExercises.FirstOrDefault(ue => ue.ExerciseId == prerequisiteExercise.Exercise.Id);

            // The prerequisite is still null when it has not been eligible to be seen yet, the postrequisite is able to be shown in that case.
            // So we don't run into a scenario where a postreq is never encountered if the prereq cannot be encountered.
            // The prerequisite's progression is >= the prerequisite's proficiency level, this exercise can be seen.
            if (prerequisiteUserExercise == null || prerequisiteUserExercise?.Progression >= viewModel.ExerciseProficiencyMap[prerequisiteExercise.Exercise.Id])
            {
                viewModel.VisiblePrerequisites.Add(prerequisiteExercise);
                continue;
            }

            viewModel.InvisiblePrerequisites.Add(prerequisiteExercise);
        }

        return View("Prerequisite", viewModel);
    }
}
