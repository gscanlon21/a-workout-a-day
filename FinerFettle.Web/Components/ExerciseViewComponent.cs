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

            viewModel.UserExercise = await scopedCoreContext.UserExercises
                .FirstOrDefaultAsync(p => p.UserId == viewModel.User.Id && p.ExerciseId == viewModel.Exercise.Id);
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
                await scopedCoreContext.SaveChangesAsync();
            }
            else
            {
                viewModel.UserExercise.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserExercises.Update(viewModel.UserExercise);
                await scopedCoreContext.SaveChangesAsync();
            }

            var userExerciseVariation = await scopedCoreContext.UserExerciseVariations
                .FirstOrDefaultAsync(p => p.UserId == viewModel.User.Id && p.ExerciseVariationId == viewModel.ExerciseVariation.Id);
            if (userExerciseVariation == null)
            {
                userExerciseVariation = new UserExerciseVariation()
                {
                    ExerciseVariationId = viewModel.ExerciseVariation.Id,
                    UserId = viewModel.User.Id,
                    LastSeen = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                scopedCoreContext.UserExerciseVariations.Add(userExerciseVariation);
                await scopedCoreContext.SaveChangesAsync();
            }
            else
            {
                userExerciseVariation.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserExerciseVariations.Update(userExerciseVariation);
                await scopedCoreContext.SaveChangesAsync();
            }

            var userVariation = await scopedCoreContext.UserVariations
                .FirstOrDefaultAsync(p => p.UserId == viewModel.User.Id && p.VariationId == viewModel.Variation.Id);
            if (userVariation == null)
            {
                userVariation = new UserVariation()
                {
                    VariationId = viewModel.Variation.Id,
                    UserId = viewModel.User.Id,
                    LastSeen = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                scopedCoreContext.UserVariations.Add(userVariation);
                await scopedCoreContext.SaveChangesAsync();
            }
            else
            {
                userVariation.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserVariations.Update(userVariation);
                await scopedCoreContext.SaveChangesAsync();
            }
        }

        // Try not to go out of the allowed range
        viewModel.HasHigherProgressionVariation = viewModel.UserExercise != null
                && viewModel.UserExercise.Progression < UserExercise.MaxUserProgression;
        viewModel.HasLowerProgressionVariation = viewModel.UserExercise != null
                && viewModel.UserExercise.Progression > UserExercise.MinUserProgression;

        return View("Exercise", viewModel);
    }
}
