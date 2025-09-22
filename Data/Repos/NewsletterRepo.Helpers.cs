using Core.Models.Exercise;
using Core.Models.User;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Data.Models.Newsletter;
using Data.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Repos;

public partial class NewsletterRepo
{
    /// <summary>
    /// Common properties surrounding today's workout.
    /// </summary>
    internal async Task<WorkoutContext?> BuildWorkoutContext(User user, string token, DateOnly date, bool isSecondWorkoutToday = false)
    {
        var (rotation, frequency) = await _userRepo.GetNextRotation(user, isSecondWorkoutToday ? Frequency.Mobility : user.ActualFrequency(date), date);
        if (rotation == null)
        {
            return null;
        }

        var (actualWeeks, weeklyMusclesRDA) = await _userRepo.GetWeeklyMuscleVolume(user, UserConsts.TrainingVolumeWeeks, rawValues: true);
        var (_, weeklyMusclesTUL) = await _userRepo.GetWeeklyMuscleVolume(user, UserConsts.TrainingVolumeWeeks, rawValues: true, tul: true);
        var userAllWorkedMuscles = (await _userRepo.GetUpcomingRotations(user, user.Frequency)).Aggregate(MusculoskeletalSystem.None, (curr, n) => curr | n.MuscleGroups.Aggregate(MusculoskeletalSystem.None, (curr2, n2) => curr2 | n2));
        var (needsDeload, timeUntilDeload) = await _userRepo.CheckNewsletterDeloadStatus(user);
        var intensity = (needsDeload, user.Intensity) switch
        {
            (true, Intensity.Light) => Intensity.Endurance,
            (true, Intensity.Medium) => Intensity.Light,
            (true, Intensity.Heavy) => Intensity.Medium,
            _ => user.Intensity,
        };

        UserLogs.Log(user, $"Weeks of data: {actualWeeks}");
        if (weeklyMusclesRDA != null && weeklyMusclesTUL != null)
        {
            UserLogs.Log(user, $"Weekly muscles RDA:{Environment.NewLine}{string.Join(Environment.NewLine, weeklyMusclesRDA)}");
            UserLogs.Log(user, $"Weekly muscles TUL:{Environment.NewLine}{string.Join(Environment.NewLine, weeklyMusclesTUL)}");
            var userMuscleMobilities = UserMuscleMobility.MuscleTargets.ToDictionary(mt => mt.Key, mt => user.UserMuscleMobilities.FirstOrDefault(umm => umm.MuscleGroup == mt.Key)?.Count ?? mt.Value);
            UserLogs.Log(user, $"Mobility targets:{Environment.NewLine}{string.Join(Environment.NewLine, userMuscleMobilities)}");
            var userMuscleStrengths = UserMuscleStrength.MuscleTargets.ToDictionary(mt => mt.Key, mt => user.UserMuscleStrengths.FirstOrDefault(umm => umm.MuscleGroup == mt.Key)?.Range ?? mt.Value);
            UserLogs.Log(user, $"Strength targets:{Environment.NewLine}{string.Join(Environment.NewLine, userMuscleStrengths)}");
            var userMuscleFlexibilities = UserMuscleFlexibility.MuscleTargets.ToDictionary(mt => mt.Key, mt => user.UserMuscleFlexibilities.FirstOrDefault(umm => umm.MuscleGroup == mt.Key)?.Count ?? mt.Value);
            UserLogs.Log(user, $"Flexibility targets:{Environment.NewLine}{string.Join(Environment.NewLine, userMuscleFlexibilities)}");
        }

        return new WorkoutContext()
        {
            Date = date,
            User = user,
            Token = token,
            Intensity = intensity,
            Frequency = frequency,
            NeedsDeload = needsDeload,
            TimeUntilDeload = timeUntilDeload,
            UserAllWorkedMuscles = userAllWorkedMuscles,
            WeeklyMusclesRDA = weeklyMusclesRDA,
            WeeklyMusclesTUL = weeklyMusclesTUL,
            WeeklyMusclesWeeks = actualWeeks,
            WorkoutRotation = rotation,
        };
    }

    /// <summary>
    /// Creates a new instance of the newsletter and saves it.
    /// </summary>
    internal async Task<UserWorkout> CreateAndAddNewsletterToContext(WorkoutContext context, IList<QueryResults> exercises)
    {
        var newsletter = new UserWorkout(context);
        // Sets the newsletter.Id after changes are saved.
        _context.UserWorkouts.Add(newsletter);
        await _context.SaveChangesAsync();

        for (var i = 0; i < exercises.Count; i++)
        {
            var exercise = exercises[i];
            _context.UserWorkoutVariations.Add(new UserWorkoutVariation(newsletter, exercise.Variation)
            {
                Section = exercise.Section,
                Order = i,
            });
        }

        await _context.SaveChangesAsync();
        return newsletter;
    }

    /// <summary>
    /// Updates the last seen date of the exercise by the user.
    /// </summary>
    /// <param name="refreshAfter">
    /// When set and the date is > Today, hold off on refreshing the LastSeen date so that we see the same exercises in each workout.
    /// </param>
    public async Task UpdateLastSeenDate(IEnumerable<QueryResults> exercises)
    {
        // Why are we doing this in a scoped db context?
        using var scope = _serviceScopeFactory.CreateScope();
        using var scopedCoreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

        foreach (var exercise in exercises.DistinctBy(e => e.UserExercise))
        {
            if (exercise.UserExercise != null)
            {
                exercise.UserExercise.LastSeen = DateHelpers.Today;
                exercise.UserExercise.FirstSeen ??= DateHelpers.Today;
                scopedCoreContext.UserExercises.Attach(exercise.UserExercise);
                scopedCoreContext.Entry(exercise.UserExercise).Property(x => x.LastSeen).IsModified = true;
                scopedCoreContext.Entry(exercise.UserExercise).Property(x => x.FirstSeen).IsModified = true;
            }
        }

        foreach (var variation in exercises.DistinctBy(e => e.UserVariation))
        {
            // >= so that today is the last day seeing the same exercises and tomorrow the exercises will refresh.
            if (variation.UserVariation != null && (variation.UserVariation.RefreshAfter == null || DateHelpers.Today >= variation.UserVariation.RefreshAfter))
            {
                // There should never be any LagRefreshXWeeks when seeing the variation for the very first time, so the else block will always run to start us off.
                var refreshAfter = variation.UserVariation.LagRefreshXWeeks == 0 ? (DateOnly?)null : DateHelpers.StartOfWeek.AddWeeks(variation.UserVariation.LagRefreshXWeeks);
                // If refresh after is today, then we will want to see a different exercise tomorrow, so we update the last seen date.
                if (variation.UserVariation.RefreshAfter == null && refreshAfter.HasValue && refreshAfter.Value > DateHelpers.Today)
                {
                    variation.UserVariation.RefreshAfter = refreshAfter.Value;
                }
                else
                {
                    variation.UserVariation.RefreshAfter = null;
                    variation.UserVariation.FirstSeen ??= DateHelpers.Today;
                    variation.UserVariation.LastSeen = DateHelpers.Today.AddWeeks(variation.UserVariation.PadRefreshXWeeks);
                }
                scopedCoreContext.UserVariations.Attach(variation.UserVariation);
                scopedCoreContext.Entry(variation.UserVariation).Property(x => x.LastSeen).IsModified = true;
                scopedCoreContext.Entry(variation.UserVariation).Property(x => x.FirstSeen).IsModified = true;
                scopedCoreContext.Entry(variation.UserVariation).Property(x => x.RefreshAfter).IsModified = true;
            }
        }

        await scopedCoreContext.SaveChangesAsync();
    }
}
