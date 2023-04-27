using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Entities.Newsletter;
using Web.Models.Newsletter;
using Web.ViewModels.Newsletter;

namespace Web.Controllers.Newsletter;

public partial class NewsletterController
{
    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    internal async Task<NewsletterRotation> GetTodaysNewsletterRotation(Entities.User.User user)
    {
        var weeklyRotation = new NewsletterTypeGroups(user.Frequency);
        var todaysNewsletterRotation = weeklyRotation.First(); // Have to start somewhere

        var previousNewsletter = await _context.Newsletters
            .Where(n => n.UserId == user.Id)
            // Get the previous newsletter from the same rotation group.
            // So that if a user switches frequencies, they continue where they left off.
            .Where(n => n.Frequency == user.Frequency)
            .OrderBy(n => n.Date)
            .ThenBy(n => n.Id) // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            .LastOrDefaultAsync();

        if (previousNewsletter != null)
        {
            todaysNewsletterRotation = weeklyRotation
                // Use Ids to compare so that a minor change to the muscle groups or movement pattern does not reset the weekly rotation
                .SkipWhile(r => r.Id != previousNewsletter.NewsletterRotation.Id)
                .Skip(1)
                .FirstOrDefault() ?? todaysNewsletterRotation;
        }

        return todaysNewsletterRotation;
    }

    /// <summary>
    /// Creates a new instance of the newsletter and saves it.
    /// </summary>
    private async Task<Entities.Newsletter.Newsletter> CreateAndAddNewsletterToContext(Entities.User.User user, NewsletterRotation newsletterRotation, bool needsDeload, IEnumerable<ExerciseViewModel> strengthExercises)
    {
        var newsletter = new Entities.Newsletter.Newsletter(Today, user, newsletterRotation, isDeloadWeek: needsDeload);
        _context.Newsletters.Add(newsletter);
        await _context.SaveChangesAsync();

        foreach (var variation in strengthExercises)
        {
            _context.NewsletterVariations.Add(new NewsletterVariation(newsletter, variation.Variation));
        }
        await _context.SaveChangesAsync();

        return newsletter;
    }

    /// <summary>
    ///     Updates the last seen date of the exercise by the user.
    /// </summary>
    /// <param name="noLog">
    ///     These get the last seen date logged to yesterday instead of today so that they are still marked seen, 
    ///     but more ?likely to make it into the main section next time.
    /// </param>
    protected async Task UpdateLastSeenDate(IEnumerable<ExerciseViewModel> exercises, IEnumerable<ExerciseViewModel> noLog, DateOnly? refreshAfter = null)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var scopedCoreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var exerciseDict = exercises.Concat(noLog).DistinctBy(e => e.Exercise).ToDictionary(e => e.Exercise);
        foreach (var exercise in exerciseDict.Keys)
        {
            DateOnly? refreshDate = (noLog.Select(vm => vm.Exercise).Contains(exercise) && refreshAfter.HasValue) ? refreshAfter.Value.AddDays(-1) : refreshAfter;
            DateOnly logDate = noLog.Select(vm => vm.Exercise).Contains(exercise) ? Today.AddDays(-1) : Today;
            if (exerciseDict[exercise].UserExercise!.RefreshAfter == null || Today > exerciseDict[exercise].UserExercise!.RefreshAfter)
            {
                if (exerciseDict[exercise].UserExercise!.RefreshAfter == null && refreshDate.HasValue)
                {
                    exerciseDict[exercise].UserExercise!.RefreshAfter = refreshDate;
                }
                else
                {
                    exerciseDict[exercise].UserExercise!.RefreshAfter = null;
                    exerciseDict[exercise].UserExercise!.LastSeen = logDate;
                }
                scopedCoreContext.UserExercises.Update(exerciseDict[exercise].UserExercise!);
            }
        }

        var exerciseVariationDict = exercises.Concat(noLog).DistinctBy(e => e.ExerciseVariation).ToDictionary(e => e.ExerciseVariation);
        foreach (var exerciseVariation in exerciseVariationDict.Keys)
        {
            DateOnly? refreshDate = (noLog.Select(vm => vm.ExerciseVariation).Contains(exerciseVariation) && refreshAfter.HasValue) ? refreshAfter.Value.AddDays(-1) : refreshAfter;
            DateOnly logDate = noLog.Select(vm => vm.ExerciseVariation).Contains(exerciseVariation) ? Today.AddDays(-1) : Today;
            if (exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter == null || Today > exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter)
            {
                if (exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter == null && refreshDate.HasValue)
                {
                    exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter = refreshDate;
                }
                else
                {
                    exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter = null;
                    exerciseVariationDict[exerciseVariation].UserExerciseVariation!.LastSeen = logDate;
                }
                scopedCoreContext.UserExerciseVariations.Update(exerciseVariationDict[exerciseVariation].UserExerciseVariation!);
            }
        }

        var variationDict = exercises.Concat(noLog).DistinctBy(e => e.Variation).ToDictionary(e => e.Variation);
        foreach (var variation in variationDict.Keys)
        {
            DateOnly? refreshDate = (noLog.Select(vm => vm.Variation).Contains(variation) && refreshAfter.HasValue) ? refreshAfter.Value.AddDays(-1) : refreshAfter;
            DateOnly logDate = noLog.Select(vm => vm.Variation).Contains(variation) ? Today.AddDays(-1) : Today;
            if (variationDict[variation].UserVariation!.RefreshAfter == null || Today > variationDict[variation].UserVariation!.RefreshAfter)
            {
                if (variationDict[variation].UserVariation!.RefreshAfter == null && refreshDate.HasValue)
                {
                    variationDict[variation].UserVariation!.RefreshAfter = refreshDate;
                }
                else
                {
                    variationDict[variation].UserVariation!.RefreshAfter = null;
                    variationDict[variation].UserVariation!.LastSeen = logDate;
                }
                scopedCoreContext.UserVariations.Update(variationDict[variation].UserVariation!);
            }
        }

        await scopedCoreContext.SaveChangesAsync();
    }
}
