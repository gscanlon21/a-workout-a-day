using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Data;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.ManageExercise;
using Web.Views.User;

namespace Web.Components.UserExercise;

/// <summary>
/// Note that we're not showing variations that don't fall in the same section.
/// This is so the user is able to manage the variations from this page.
/// </summary>
public class ManageExerciseViewComponent : ViewComponent
{
    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "ManageExercise";

    private readonly CoreContext _context;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ManageExerciseViewComponent(CoreContext context, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, ManageExerciseVariationViewModel.Params parameters)
    {
        // UserExercise's are created when querying for an exercise.
        var userExercise = await _context.UserExercises
            .Include(ue => ue.Exercise)
            .Where(ue => ue.UserId == user.Id)
            .Where(ue => ue.ExerciseId == parameters.ExerciseId)
            .IgnoreQueryFilters().FirstOrDefaultAsync();

        if (userExercise == null) { return Content(""); }
        var exerciseVariations = await new QueryBuilder(parameters.Section)
            // Need to pass in a user to show the user's progression level.
            .WithUser(user, ignoreSoftFiltering: true)
            .WithExercises(x =>
            {
                x.AddExercises([userExercise]);
            })
            .Build()
            .Query(_serviceScopeFactory);

        // Need to query in the same section so user can manage it.
        var allExerciseVariations = (await new QueryBuilder(parameters.Section)
            .WithExercises(x =>
            {
                x.AddExercises([userExercise]);
            })
            .Build()
            .Query(_serviceScopeFactory))
            .ToList();

        // Let the user know that they are not seeing these variations.
        allExerciseVariations.ForEach(vm => vm.Theme = ExerciseTheme.None);

        // Showing all variations even if the user cannot see them so they can find out why.
        var finalExerciseVariations = exerciseVariations.UnionBy(allExerciseVariations, vm => vm.Variation);
        if (!finalExerciseVariations.Any()) { return Content(""); }

        // Need a user context so the manage link is clickable and the user can un-ignore an exercise/variation.
        var userNewsletter = new UserNewsletterDto(user.AsType<UserDto>()!, parameters.Token)
        {
            // Show all instructions.
            Equipment = Equipment.All,
            // Don't show proficiency.
            Intensity = Intensity.None
        };

        return View("ManageExercise", new ManageExerciseViewModel()
        {
            User = user,
            Parameters = parameters,
            UserExercise = userExercise,
            UserNewsletter = userNewsletter,
            Exercise = userExercise.Exercise,
            ExerciseVariations = finalExerciseVariations.Select(r => r.AsType<ExerciseVariationDto>()!).ToList(),
        });
    }
}