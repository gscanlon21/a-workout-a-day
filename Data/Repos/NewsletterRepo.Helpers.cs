using Core.Code;
using Core.Consts;
using Core.Dtos.User;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Data.Query;
using Microsoft.Extensions.DependencyInjection;
using Web.Code;

namespace Data.Repos;

public partial class NewsletterRepo
{
    /// <summary>
    /// Common properties surrounding today's workout.
    /// </summary>
    internal async Task<WorkoutContext?> BuildWorkoutContext(User user, string token)
    {
        var (frequency, rotation) = await userRepo.GetNextRotation(user);
        if (rotation == null)
        {
            return null;
        }

        var (actualWeeks, weeklyMusclesRDA) = await userRepo.GetWeeklyMuscleVolume(user, UserConsts.TrainingVolumeWeeks, rawValues: true);
        var (_, weeklyMusclesTUL) = await userRepo.GetWeeklyMuscleVolume(user, UserConsts.TrainingVolumeWeeks, rawValues: true, tul: true);
        var userAllWorkedMuscles = (await userRepo.GetUpcomingRotations(user, user.Frequency)).Aggregate(MuscleGroups.None, (curr, n) => curr | n.MuscleGroups.Aggregate(MuscleGroups.None, (curr2, n2) => curr2 | n2));
        var (needsDeload, timeUntilDeload) = await userRepo.CheckNewsletterDeloadStatus(user);
        var intensity = (needsDeload, user.Intensity) switch
        {
            (true, Intensity.Light) => Intensity.Endurance,
            (true, Intensity.Medium) => Intensity.Light,
            (true, Intensity.Heavy) => Intensity.Medium,
            _ => user.Intensity,
        };

        Logs.AppendLog(user, $"Weeks of data: {actualWeeks}");
        if (weeklyMusclesRDA != null && weeklyMusclesTUL != null)
        {
            Logs.AppendLog(user, $"Weekly muscles RDA:{Environment.NewLine}{string.Join(Environment.NewLine, weeklyMusclesRDA)}");
            Logs.AppendLog(user, $"Weekly muscles TUL:{Environment.NewLine}{string.Join(Environment.NewLine, weeklyMusclesTUL)}");
            var userMuscleMobilities = UserMuscleMobility.MuscleTargets.ToDictionary(mt => mt.Key, mt => user.UserMuscleMobilities.FirstOrDefault(umm => umm.MuscleGroup == mt.Key)?.Count ?? mt.Value);
            Logs.AppendLog(user, $"Mobility targets:{Environment.NewLine}{string.Join(Environment.NewLine, userMuscleMobilities)}");
            var userMuscleStrengths = UserMuscleStrength.MuscleTargets.ToDictionary(mt => mt.Key, mt => user.UserMuscleStrengths.FirstOrDefault(umm => umm.MuscleGroup == mt.Key)?.Range ?? mt.Value);
            Logs.AppendLog(user, $"Strength targets:{Environment.NewLine}{string.Join(Environment.NewLine, userMuscleStrengths)}");
            var userMuscleFlexibilities = UserMuscleFlexibility.MuscleTargets.ToDictionary(mt => mt.Key, mt => user.UserMuscleFlexibilities.FirstOrDefault(umm => umm.MuscleGroup == mt.Key)?.Count ?? mt.Value);
            Logs.AppendLog(user, $"Flexibility targets:{Environment.NewLine}{string.Join(Environment.NewLine, userMuscleFlexibilities)}");
        }

        return new WorkoutContext()
        {
            User = user.AsType<UserDto, User>()!,
            Token = token,
            Intensity = intensity,
            Frequency = frequency,
            NeedsDeload = needsDeload,
            TimeUntilDeload = timeUntilDeload,
            UserAllWorkedMuscles = userAllWorkedMuscles,
            WorkoutRotation = rotation,
            WeeklyMusclesRDA = weeklyMusclesRDA,
            WeeklyMusclesTUL = weeklyMusclesTUL,
            WeeklyMusclesWeeks = actualWeeks,
        };
    }

    /// <summary>
    /// Creates a new instance of the newsletter and saves it.
    /// </summary>
    internal async Task<UserWorkout> CreateAndAddNewsletterToContext(WorkoutContext context, IList<QueryResults>? exercises = null)
    {
        var newsletter = new UserWorkout(context.User.TodayOffset, context);
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
    public async Task UpdateLastSeenDate(IEnumerable<QueryResults> exercises)
    {
        using var scope = serviceScopeFactory.CreateScope();
        using var scopedCoreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

        foreach (var exercise in exercises.DistinctBy(e => e.UserExercise))
        {
            if (exercise.UserExercise != null)
            {
                exercise.UserExercise.LastSeen = DateHelpers.Today;
                scopedCoreContext.UserExercises.Update(exercise.UserExercise);
            }
        }

        foreach (var variation in exercises.DistinctBy(e => e.UserVariation))
        {
            // >= so that today is the last day seeing the same exercises and tomorrow the exercises will refresh.
            if (variation.UserVariation != null && (variation.UserVariation.RefreshAfter == null || DateHelpers.Today >= variation.UserVariation.RefreshAfter))
            {
                var refreshAfter = variation.UserVariation.LagRefreshXWeeks == 0 ? (DateOnly?)null : DateHelpers.StartOfWeek.AddDays(7 * variation.UserVariation.LagRefreshXWeeks);
                // If refresh after is today, we want to see a different exercises tomorrow so update the last seen date.
                if (variation.UserVariation.RefreshAfter == null && refreshAfter.HasValue && refreshAfter.Value > DateHelpers.Today)
                {
                    variation.UserVariation.RefreshAfter = refreshAfter.Value;
                }
                else
                {
                    variation.UserVariation.RefreshAfter = null;
                    variation.UserVariation.LastSeen = DateHelpers.Today.AddDays(7 * variation.UserVariation.PadRefreshXWeeks);
                }
                scopedCoreContext.UserVariations.Update(variation.UserVariation);
            }
        }

        await scopedCoreContext.SaveChangesAsync();
    }
}
