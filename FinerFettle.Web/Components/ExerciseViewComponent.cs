using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.ViewModels.User;
using FinerFettle.Web.Extensions;

namespace FinerFettle.Web.Components
{
    public class ExerciseViewComponent : ViewComponent
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExerciseViewComponent(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(UserNewsletterViewModel? user, ExerciseViewModel viewModel)
        {
            if (viewModel == null)
            {
                return Content(string.Empty);
            }

            if (user != null)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var coreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

                viewModel.UserExercise = await coreContext.UserExercises
                    .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseId == viewModel.Exercise.Id);
                if (viewModel.UserExercise == null)
                {
                    viewModel.UserExercise = new UserExercise()
                    {
                        ExerciseId = viewModel.Exercise.Id,
                        UserId = user.Id,
                        Progression = viewModel.Exercise.IsRecovery ? UserExercise.MinUserProgression : MathExtensions.RoundToX(UserExercise.RoundToNearestX, user.AverageProgression),
                        LastSeen = DateOnly.FromDateTime(DateTime.UtcNow)
                    };

                    coreContext.UserExercises.Add(viewModel.UserExercise);
                    await coreContext.SaveChangesAsync();
                }
                else
                {
                    viewModel.UserExercise.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                    coreContext.UserExercises.Update(viewModel.UserExercise);
                    await coreContext.SaveChangesAsync();
                }

                var userVariation = await coreContext.UserVariations
                    .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VariationId == viewModel.Variation.Id);
                if (userVariation == null)
                {
                    userVariation = new UserVariation()
                    {
                        VariationId = viewModel.Variation.Id,
                        UserId = user.Id,
                        LastSeen = DateOnly.FromDateTime(DateTime.UtcNow)
                    };
                    
                    coreContext.UserVariations.Add(userVariation);
                    await coreContext.SaveChangesAsync();
                }
                else
                {
                    userVariation.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                    coreContext.UserVariations.Update(userVariation);
                    await coreContext.SaveChangesAsync();
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
}
