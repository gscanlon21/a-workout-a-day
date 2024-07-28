using Core.Dtos.Newsletter;
using Core.Models.Newsletter;
using Data;
using Data.Query;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code;
using Web.Views.Shared.Components.ManageExercise;
using Web.Views.User;

namespace Web.Components.UserExercise;

public class ManageExerciseViewComponent(CoreContext context, IServiceScopeFactory serviceScopeFactory) : ViewComponent
{
    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "ManageExercise";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, ManageExerciseVariationDto.Params parameters)
    {
        // UserExercise's are created when querying for an exercise.
        var userExercise = await context.UserExercises
            .IgnoreQueryFilters()
            .Include(ue => ue.Exercise)
            .Where(ue => ue.UserId == user.Id)
            .FirstOrDefaultAsync(ue => ue.ExerciseId == parameters.ExerciseId);

        if (userExercise == null) { return Content(""); }
        var exerciseVariations = await new QueryBuilder(Section.None)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, ignoreIgnored: true, ignoreMissingEquipment: true, uniqueExercises: false)
            .WithExercises(x =>
            {
                x.AddExercises([userExercise.Exercise]);
            })
            .Build()
            .Query(serviceScopeFactory);

        if (!exerciseVariations.Any()) { return Content(""); }
        return View("ManageExercise", new ManageExerciseViewModel()
        {
            User = user,
            Parameters = parameters,
            UserExercise = userExercise,
            Exercise = userExercise.Exercise,
            ExerciseVariations = exerciseVariations.Select(r => r.AsType<ExerciseVariationDto, QueryResults>()!).ToList(),
        });
    }
}