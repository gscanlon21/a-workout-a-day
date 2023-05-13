using Microsoft.EntityFrameworkCore;
using Web.Code.Extensions;
using Web.Data;
using Web.Entities.Newsletter;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;
using Web.ViewModels.Newsletter;

namespace Web.Controllers.Newsletter;

public partial class NewsletterController
{
    /// <summary>
    /// The exercise query runner requires UserExercise/UserExerciseVariation/UserVariation records to have already been made.
    /// There is a small chance for a race-condition if Exercise/ExerciseVariation/Variation data is updated after these run in.
    /// I am not concerned about that possiblity because the data changes infrequently and the newsletter will with the next trigger.
    /// </summary>
    internal async Task AddMissingUserExerciseVariationRecords(Entities.User.User user)
    {
        _context.AddMissing(await _context.UserExercises.Where(ue => ue.UserId == user.Id).Select(ue => ue.ExerciseId).ToListAsync(),
            await _context.Exercises.Select(e => new { e.Id, e.Proficiency }).ToListAsync(), k => k.Id, e => new UserExercise() { ExerciseId = e.Id, UserId = user.Id, Progression = user.IsNewToFitness ? UserExercise.MinUserProgression : e.Proficiency });

        _context.AddMissing(await _context.UserExerciseVariations.Where(ue => ue.UserId == user.Id).Select(uev => uev.ExerciseVariationId).ToListAsync(),
            await _context.ExerciseVariations.Select(ev => ev.Id).ToListAsync(), evId => new UserExerciseVariation() { ExerciseVariationId = evId, UserId = user.Id });

        _context.AddMissing(await _context.UserVariations.Where(ue => ue.UserId == user.Id).Select(uv => uv.VariationId).ToListAsync(),
            await _context.Variations.Select(v => v.Id).ToListAsync(), vId => new UserVariation() { VariationId = vId, UserId = user.Id });

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    internal async Task<NewsletterRotation> GetTodaysNewsletterRotation(Entities.User.User user)
    {
        return await GetTodaysNewsletterRotation(user.Id, user.Frequency);
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    internal async Task<NewsletterRotation> GetTodaysNewsletterRotation(int userId, Models.User.Frequency frequency)
    {
        var weeklyRotation = new NewsletterTypeGroups(frequency);
        var todaysNewsletterRotation = weeklyRotation.First(); // Have to start somewhere

        var previousNewsletter = await _context.Newsletters
            .Where(n => n.UserId == userId)
            // Get the previous newsletter from the same rotation group.
            // So that if a user switches frequencies, they continue where they left off.
            .Where(n => n.Frequency == frequency)
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
    private async Task<Entities.Newsletter.Newsletter> CreateAndAddNewsletterToContext(Entities.User.User user, NewsletterRotation newsletterRotation, Frequency frequency, bool needsDeload, IEnumerable<ExerciseViewModel> strengthExercises)
    {
        var newsletter = new Entities.Newsletter.Newsletter(Today, user, newsletterRotation, frequency, isDeloadWeek: needsDeload);
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
    /// 
    /// </summary>
    public IntensityLevel ToIntensityLevel(IntensityLevel userIntensityLevel, bool deload = false)
    {
        if (deload)
        {
            return userIntensityLevel switch
            {
                IntensityLevel.Light => IntensityLevel.Endurance,
                IntensityLevel.Medium => IntensityLevel.Light,
                IntensityLevel.Heavy => IntensityLevel.Medium,
                _ => throw new NotImplementedException()
            };
        }

        return userIntensityLevel switch
        {
            IntensityLevel.Light => IntensityLevel.Light,
            IntensityLevel.Medium => IntensityLevel.Medium,
            IntensityLevel.Heavy => IntensityLevel.Heavy,
            _ => throw new NotImplementedException()
        };
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

        await scopedCoreContext.SaveChangesAsync();
    }
}
