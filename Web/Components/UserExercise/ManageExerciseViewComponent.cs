using Core.Dtos.Newsletter;
using Core.Models.Newsletter;
using Data;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.ManageExercise;
using Web.Views.User;

namespace Web.Components.UserExercise;

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
            .IgnoreQueryFilters()
            .Include(ue => ue.Exercise)
            .Where(ue => ue.UserId == user.Id)
            .FirstOrDefaultAsync(ue => ue.ExerciseId == parameters.ExerciseId);

        if (userExercise == null) { return Content(""); }
        var exerciseVariations = await new QueryBuilder(Section.None)
            .WithExercises(x =>
            {
                x.AddExercises([userExercise]);
            })
            .Build()
            .Query(_serviceScopeFactory);

        if (!exerciseVariations.Any()) { return Content(""); }
        return View("ManageExercise", new ManageExerciseViewModel()
        {
            User = user,
            Parameters = parameters,
            UserExercise = userExercise,
            Exercise = userExercise.Exercise,
            ExerciseVariations = exerciseVariations.Select(r => r.AsType<ExerciseVariationDto>()!).ToList(),
        });
    }
}