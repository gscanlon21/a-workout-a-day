using FinerFettle.Web.Data;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
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

            if (viewModel.UserExerciseVariation == null)
            {
                viewModel.UserExerciseVariation = new UserExerciseVariation()
                {
                    ExerciseVariationId = viewModel.ExerciseVariation.Id,
                    UserId = viewModel.User.Id,
                    LastSeen = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                scopedCoreContext.UserExerciseVariations.Add(viewModel.UserExerciseVariation);
            }
            else
            {
                viewModel.UserExerciseVariation.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserExerciseVariations.Update(viewModel.UserExerciseVariation);
            }

            if (viewModel.UserVariation == null)
            {
                viewModel.UserVariation = new UserVariation()
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
                viewModel.UserVariation.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserVariations.Update(viewModel.UserVariation);
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
