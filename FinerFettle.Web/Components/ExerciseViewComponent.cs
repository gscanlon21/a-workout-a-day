using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.ViewModels.User;

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
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var coreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

                    viewModel.UserProgression = await coreContext.UserProgressions
                        .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseId == viewModel.Exercise.Id);

                    if (viewModel.UserProgression == null)
                    {
                        viewModel.UserProgression = new ExerciseUserProgression()
                        {
                            ExerciseId = viewModel.Exercise.Id,
                            UserId = user.Id,
                            Progression = 5 * (int)Math.Round(user.AverageProgression / 5d)
                        };

                        coreContext.UserProgressions.Add(viewModel.UserProgression);
                        await coreContext.SaveChangesAsync();
                    }
                }
            }

            // Try not to go out of the allowed range
            viewModel.HasHigherProgressionVariation = viewModel.UserProgression != null
                    && viewModel.UserProgression.Progression < 95;
            viewModel.HasLowerProgressionVariation = viewModel.UserProgression != null
                    && viewModel.UserProgression.Progression > 5;

            return View("Exercise", viewModel);
        }
    }
}
