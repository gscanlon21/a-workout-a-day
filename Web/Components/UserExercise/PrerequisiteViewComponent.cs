using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Data;
using Data.Query;
using Data.Query.Builders;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code;
using Web.Views.Shared.Components.Prerequisite;
using Web.Views.User;

namespace Web.Components.UserExercise;

/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class PrerequisiteViewComponent : ViewComponent
{
    private readonly UserRepo _userRepo;
    private readonly CoreContext _context;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PrerequisiteViewComponent(IServiceScopeFactory serviceScopeFactory, CoreContext context, UserRepo userRepo)
    {
        _context = context;
        _userRepo = userRepo;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "Prerequisite";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, ManageExerciseVariationDto.Params parameters)
    {
        var prerequisites = await _context.ExercisePrerequisites
            .Include(ep => ep.PrerequisiteExercise)
            .Where(ep => ep.ExerciseId == parameters.ExerciseId)
            .ToListAsync();

        // Variations missing equipment or that are ignored are not shown, since they will never be seen.
        var prerequisiteExercises = (await new QueryBuilder()
            .WithUser(user, ignorePrerequisites: true, ignoreProgressions: true, uniqueExercises: false)
            .WithExercises(builder =>
            {
                builder.AddExercises(prerequisites.Select(p => p.PrerequisiteExercise));
            })
            .Build()
            .Query(_serviceScopeFactory))
            .Select(r => r.AsType<ExerciseVariationDto, QueryResults>()!)
            .ToList();

        // Need a user context so the manage link is clickable and the user can un-ignore an exercise/variation.
        var userNewsletter = user.AsType<UserNewsletterDto, Data.Entities.User.User>()!;
        userNewsletter.Token = await _userRepo.AddUserToken(user, durationDays: 1);
        var viewModel = new PrerequisiteViewModel()
        {
            UserNewsletter = userNewsletter,
            Prerequisites = prerequisites,
            VisiblePrerequisites = [],
            InvisiblePrerequisites = []
        };

        var userExercises = await _context.UserExercises
            .Where(ue => ue.UserId == user.Id)
            .Where(ue => prerequisites.Select(p => p.PrerequisiteExerciseId).Contains(ue.ExerciseId))
            .ToListAsync();

        foreach (var prerequisiteExercise in prerequisiteExercises)
        {
            var prerequisite = prerequisites.First(p => p.PrerequisiteExerciseId == prerequisiteExercise.Exercise.Id);
            var prerequisiteUserExercise = userExercises.FirstOrDefault(ue => ue.ExerciseId == prerequisiteExercise.Exercise.Id);

            // The prerequisite is still null when it has not been eligible to be seen yet, the postrequisite is able to be shown in that case.
            // So we don't run into a scenario where a postreq is never encountered if the prereq cannot be encountered.
            if (prerequisiteUserExercise == null
                // The prerequisite's progression is >= the prerequisite's proficiency level, this exercise can be seen.
                || prerequisiteUserExercise?.Progression >= prerequisite.Proficiency)
            {
                viewModel.VisiblePrerequisites.Add(prerequisiteExercise);
                continue;
            }

            viewModel.InvisiblePrerequisites.Add(prerequisiteExercise);
        }

        return View("Prerequisite", viewModel);
    }
}
