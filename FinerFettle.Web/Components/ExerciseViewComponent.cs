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
        private readonly CoreContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExerciseViewComponent(CoreContext context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
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
            bool isUserProgressionInRange = viewModel.UserProgression != null
                    && viewModel.UserProgression.Progression < 95
                    && viewModel.UserProgression.Progression > 5;

            // You should be able to progress above an exercise that has a max progression set
            viewModel.HasHigherProgressionVariation = isUserProgressionInRange && (
                // In case the exercise was allowed by the user's average progression:
                // Don't show if the exercise progression is already above the max progression.
                (viewModel.Intensity.Progression.Max.HasValue && viewModel.UserProgression!.Progression < viewModel.Intensity.Progression.Max)
                || 
                // In case the exercise was allowed by the user's average progression:
                // Show if the exercise progression is below the min progression so the user can progress back into range.
                (viewModel.Intensity.Progression.Min.HasValue && viewModel.UserProgression!.Progression < viewModel.Intensity.Progression.Min)
            );

            // You should be able to progress below an exercise that has a min progression set
            viewModel.HasLowerProgressionVariation = isUserProgressionInRange && (
                // In case the exercise was allowed by the user's average progression:
                // Don't show if the exercise progression is already below the min progression.
                (viewModel.Intensity.Progression.Min.HasValue && viewModel.UserProgression!.Progression >= viewModel.Intensity.Progression.Min)
                ||
                // In case the exercise was allowed by the user's average progression:
                // Show if the exercise progression is above the max progression so the user can progress back into range.
                (viewModel.Intensity.Progression.Max.HasValue && viewModel.UserProgression!.Progression >= viewModel.Intensity.Progression.Max)
            );

            return View("Exercise", viewModel);
        }
    }
}
