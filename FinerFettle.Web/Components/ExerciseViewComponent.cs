using FinerFettle.Web.Data;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;

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

        public async Task<IViewComponentResult> InvokeAsync(User? user, ExerciseViewModel exercise, bool verbose = false)
        {
            if (exercise == null)
            {
                return Content(string.Empty);
            }

            if (user != null)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var coreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

                    exercise.UserProgression = await coreContext.UserProgressions
                        .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseId == exercise.Exercise.Id);

                    if (exercise.UserProgression == null)
                    {
                        exercise.UserProgression = new ExerciseUserProgression()
                        {
                            ExerciseId = exercise.Exercise.Id,
                            UserId = user.Id,
                            Progression = 50 // FIXME: Magic int is magic. But really just the mid progression level.
                        };

                        coreContext.UserProgressions.Add(exercise.UserProgression);
                        await coreContext.SaveChangesAsync();
                    }
                }
            }

            // You should be able to progress above an exercise that has a max progression set
            exercise.HasHigherProgressionVariation = exercise.Intensity.Progression.Max != null 
                && exercise.UserProgression != null
                // Try not to go out of the allowed range
                && exercise.UserProgression.Progression < 95
                // In case the exercise was allowed by the user's average progression:
                // Don't show if the exercise progression is already above the max progression.
                && exercise.UserProgression.Progression < exercise.Intensity.Progression.Max;

            // You should be able to progress below an exercise that has a min progression set
            exercise.HasLowerProgressionVariation = exercise.Intensity.Progression.Min != null 
                && exercise.UserProgression != null 
                // Try not to go out of the allowed range
                && exercise.UserProgression.Progression > 5
                // In case the exercise was allowed by the user's average progression:
                // Don't show if the exercise progression is already below the min progression.
                && exercise.UserProgression.Progression > exercise.Intensity.Progression.Min;

            exercise.Verbose = verbose;

            return View("Exercise", exercise);
        }
    }
}
