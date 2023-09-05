using Core.Consts;
using Core.Models.Exercise;
using Data.Dtos.Newsletter;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Data.Models.Newsletter;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Repos;

public partial class NewsletterRepo
{
    /// <summary>
    /// Common properties surrounding today's workout.
    /// </summary>
    internal async Task<WorkoutContext?> BuildWorkoutContext(User user, string token)
    {
        var (frequency, rotation) = await _userRepo.GetNextRotation(user);
        if (rotation == null)
        {
            return null;
        }

        // Add 1 because deloads occur after every x weeks, not on.
        var (actualWeeks, weeklyMuscles) = await _userRepo.GetWeeklyMuscleVolume(user, weeks: Math.Max(UserConsts.DeloadAfterEveryXWeeksDefault, user.DeloadAfterEveryXWeeks + 1));
        var userAllWorkedMuscles = (await _userRepo.GetUpcomingRotations(user, user.Frequency)).Aggregate(MuscleGroups.None, (curr, n) => curr | n.MuscleGroups.Aggregate(MuscleGroups.None, (curr2, n2) => curr2 | n2));
        var (needsDeload, timeUntilDeload) = await _userRepo.CheckNewsletterDeloadStatus(user);

        return new WorkoutContext()
        {
            User = user,
            Token = token,
            Frequency = frequency,
            NeedsDeload = needsDeload,
            TimeUntilDeload = timeUntilDeload,
            UserAllWorkedMuscles = userAllWorkedMuscles,
            WorkoutRotation = rotation,
            WeeklyMuscles = weeklyMuscles,
            WeeklyMusclesWeeks = actualWeeks,
        };
    }

    /// <summary>
    /// Creates a new instance of the newsletter and saves it.
    /// </summary>
    internal async Task<UserWorkout> CreateAndAddNewsletterToContext(WorkoutContext context, IList<ExerciseVariationDto>? exercises = null)
    {
        var newsletter = new UserWorkout(Today, context);
        _context.UserWorkouts.Add(newsletter); // Sets the newsletter.Id after changes are saved.
        await _context.SaveChangesAsync();

        if (exercises != null)
        {
            for (var i = 0; i < exercises.Count; i++)
            {
                var exercise = exercises[i];
                _context.UserWorkoutVariations.Add(new UserWorkoutVariation(newsletter, exercise.Variation)
                {
                    Section = exercise.Section,
                    Order = i,
                });
            }
        }

        await _context.SaveChangesAsync();
        return newsletter;
    }

    /// <summary>
    ///     Updates the last seen date of the exercise by the user.
    /// </summary>
    /// <param name="refreshAfter">
    ///     When set and the date is > Today, hold off on refreshing the LastSeen date so that we see the same exercises in each workout.
    /// </param>
    public async Task UpdateLastSeenDate(IEnumerable<ExerciseVariationDto> exercises, DateOnly? refreshAfter = null)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        using var scopedCoreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

        foreach (var userExercise in exercises.Select(e => e.UserExercise).Distinct())
        {
            // >= so that today is the last day seeing the same exercises and tomorrow the exercises will refresh.
            if (userExercise != null && (userExercise.RefreshAfter == null || Today >= userExercise.RefreshAfter))
            {
                // If refresh after is today, we want to see a different exercises tomorrow so update the last seen date.
                if (userExercise.RefreshAfter == null && refreshAfter.HasValue && refreshAfter.Value > Today)
                {
                    userExercise.RefreshAfter = refreshAfter.Value;
                }
                else
                {
                    userExercise.RefreshAfter = null;
                    userExercise.LastSeen = Today;
                }
                scopedCoreContext.UserExercises.Update(userExercise);
            }
        }

        foreach (var userVariation in exercises.Select(e => e.UserVariation).Distinct())
        {
            // >= so that today is the last day seeing the same exercises and tomorrow the exercises will refresh.
            if (userVariation != null && (userVariation.RefreshAfter == null || Today >= userVariation.RefreshAfter))
            {
                // If refresh after is today, we want to see a different exercises tomorrow so update the last seen date.
                if (userVariation.RefreshAfter == null && refreshAfter.HasValue && refreshAfter.Value > Today)
                {
                    userVariation.RefreshAfter = refreshAfter.Value;
                }
                else
                {
                    userVariation.RefreshAfter = null;
                    userVariation.LastSeen = Today;
                }
                scopedCoreContext.UserVariations.Update(userVariation);
            }
        }

        await scopedCoreContext.SaveChangesAsync();
    }
}
