using Core.Consts;
using Core.Models.Newsletter;
using Data;
using Data.Query;
using Data.Query.Builders;
using Lib.Pages.Shared.Exercise;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code;
using Web.Views.Shared.Components.ManageExercise;
using Web.Views.User;

namespace Web.Components.UserExercise;

public class ManageExerciseViewComponent(CoreContext context, IServiceScopeFactory serviceScopeFactory) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "ManageExercise";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, ManageExerciseVariationViewModel.Params parameters)
    {
        var userExercise = await context.UserExercises
            .IgnoreQueryFilters()
            .Include(ue => ue.Exercise)
            .Where(ue => ue.UserId == user.Id)
            .FirstOrDefaultAsync(ue => ue.ExerciseId == parameters.ExerciseId);

        if (userExercise == null)
        {
            userExercise = new Data.Entities.User.UserExercise()
            {
                UserId = user.Id,
                ExerciseId = parameters.ExerciseId,
                Progression = user.IsNewToFitness ? UserConsts.MinUserProgression : UserConsts.MidUserProgression,
                Exercise = await context.Exercises.FirstAsync(e => e.Id == parameters.ExerciseId),
            };

            context.UserExercises.Add(userExercise);
            await context.SaveChangesAsync();
        }

        var exerciseVariations = (await new QueryBuilder(Section.None)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, ignoreIgnored: true, ignoreMissingEquipment: true, uniqueExercises: false)
            .WithExercises(x =>
            {
                x.AddExercises([userExercise.Exercise]);
            })
            .Build()
            .Query(serviceScopeFactory))
            // Order by progression levels
            .OrderBy(vm => vm.Variation.Progression.Min)
            .ThenBy(vm => vm.Variation.Progression.Max == null)
            .ThenBy(vm => vm.Variation.Progression.Max)
            .ThenBy(vm => vm.Variation.Name)
            .Select(r => r.AsType<ExerciseVariationViewModel, QueryResults>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        return View("ManageExercise", new ManageExerciseViewModel()
        {
            Parameters = parameters,
            User = user,
            Exercise = userExercise.Exercise,
            ExerciseVariations = exerciseVariations,
            UserExercise = userExercise,
        });
    }
}