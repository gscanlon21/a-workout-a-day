using FinerFettle.Web.Data;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Entities.User;

namespace FinerFettle.Web.Components;

public class ExerciseViewComponent : ViewComponent
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ExerciseViewComponent(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<IViewComponentResult> InvokeAsync(ExerciseViewModel viewModel)
    {
        if (viewModel == null)
        {
            return Content(string.Empty);
        }

        if (viewModel.User != null)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var scopedCoreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

            var queryResults = await scopedCoreContext.Users
                .Select(u => new {
                    UserId = u.Id,
                    UserExercise = u.UserExercises.FirstOrDefault(ue => ue.ExerciseId == viewModel.Exercise.Id),
                    UserExerciseVariation = u.UserExerciseVariations.FirstOrDefault(ue => ue.ExerciseVariationId == viewModel.ExerciseVariation.Id),
                    UserVariation = u.UserVariations.FirstOrDefault(ue => ue.VariationId == viewModel.Variation.Id),
                })
                .FirstAsync(p => p.UserId == viewModel.User.Id);

            viewModel.UserExercise = queryResults.UserExercise;
            if (viewModel.UserExercise == null)
            {
                viewModel.UserExercise = new UserExercise()
                {
                    ExerciseId = viewModel.Exercise.Id,
                    UserId = viewModel.User.Id,
                    Progression = UserExercise.MinUserProgression,
                    LastSeen = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                scopedCoreContext.UserExercises.Add(viewModel.UserExercise);
            }
            else
            {
                viewModel.UserExercise.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserExercises.Update(viewModel.UserExercise);
            }

            if (queryResults.UserExerciseVariation == null)
            {
                scopedCoreContext.UserExerciseVariations.Add(new UserExerciseVariation()
                {
                    ExerciseVariationId = viewModel.ExerciseVariation.Id,
                    UserId = viewModel.User.Id,
                    LastSeen = DateOnly.FromDateTime(DateTime.UtcNow)
                });
            }
            else
            {
                queryResults.UserExerciseVariation.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserExerciseVariations.Update(queryResults.UserExerciseVariation);
            }

            if (queryResults.UserVariation == null)
            {
                scopedCoreContext.UserVariations.Add(new UserVariation()
                {
                    VariationId = viewModel.Variation.Id,
                    UserId = viewModel.User.Id,
                    LastSeen = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                scopedCoreContext.UserVariations.Add(viewModel.UserVariation);

                // First time the user has seen this exercise,
                // decrease the number of sets the user has to perform so they can focus on form.
                viewModel.Variation.Intensities.ForEach(i => i.Proficiency.Sets = 1);
            }
            else
            {
                queryResults.UserVariation.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserVariations.Update(queryResults.UserVariation);
            }

            await scopedCoreContext.SaveChangesAsync();
        }

        // Try not to go out of the allowed range
        viewModel.HasHigherProgressionVariation = viewModel.UserExercise != null
                && viewModel.UserExercise.Progression < UserExercise.MaxUserProgression;
        viewModel.HasLowerProgressionVariation = viewModel.UserExercise != null
                && viewModel.UserExercise.Progression > UserExercise.MinUserProgression;

        return View("Exercise", viewModel);
    }
}
