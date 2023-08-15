using Core.Consts;
using Core.Models.Exercise;
using Data.Dtos.Newsletter;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Data.Models.Newsletter;
using Microsoft.EntityFrameworkCore;
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
        var userAllWorkedMuscles = (await _userRepo.GetUpcomingRotations(user, user.Frequency)).Aggregate(MuscleGroups.None, (curr, n) => curr | n.MuscleGroups);
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
    /// The exercise query runner requires UserExercise/UserExerciseVariation/UserVariation records to have already been made.
    /// There is a small chance for a race-condition if Exercise/ExerciseVariation/Variation records are added after these run in.
    /// I'm not concerned about that possiblity because the data changes infrequently, and the newsletter will resend with the next trigger (twice-hourly).
    /// </summary>
    internal async Task AddMissingUserExerciseVariationRecords(User user)
    {
        // When EF Core allows batching seperate queries, refactor this.
        var missingUserExercises = await _context.Exercises.TagWithCallSite()
            .Where(e => !_context.UserExercises.Where(ue => ue.UserId == user.Id).Select(ue => ue.ExerciseId).Contains(e.Id))
            .Select(e => new { e.Id, e.Proficiency })
            .ToListAsync();

        var missingUserExerciseVariationIds = await _context.ExerciseVariations.TagWithCallSite()
            .Where(e => !_context.UserExerciseVariations.Where(ue => ue.UserId == user.Id).Select(ue => ue.ExerciseVariationId).Contains(e.Id))
            .Select(ev => ev.Id)
            .ToListAsync();

        var missingUserVariationIds = await _context.Variations.TagWithCallSite()
            .Where(e => !_context.UserVariations.Where(ue => ue.UserId == user.Id).Select(ue => ue.VariationId).Contains(e.Id))
            .Select(v => v.Id)
            .ToListAsync();

        // Add missing User* records
        _context.UserExercises.AddRange(missingUserExercises.Select(e => new UserExercise() { ExerciseId = e.Id, UserId = user.Id, Progression = user.IsNewToFitness ? UserConsts.MinUserProgression : e.Proficiency }));
        _context.UserExerciseVariations.AddRange(missingUserExerciseVariationIds.Select(evId => new UserExerciseVariation() { ExerciseVariationId = evId, UserId = user.Id }));
        _context.UserVariations.AddRange(missingUserVariationIds.Select(vId => new UserVariation() { VariationId = vId, UserId = user.Id }));

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a new instance of the newsletter and saves it.
    /// </summary>
    internal async Task<UserWorkout> CreateAndAddNewsletterToContext(WorkoutContext context, IList<ExerciseDto>? exercises = null)
    {
        var newsletter = new UserWorkout(Today, context);
        _context.UserWorkouts.Add(newsletter); // Sets the newsletter.Id after changes are saved.
        await _context.SaveChangesAsync();

        if (exercises != null)
        {
            for (var i = 0; i < exercises.Count; i++)
            {
                var exercise = exercises[i];
                _context.UserWorkoutExerciseVariations.Add(new UserWorkoutExerciseVariation(newsletter, exercise.ExerciseVariation)
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
    /// 
    /// </summary>
    public static Intensity ToIntensity(Intensity userIntensity, bool lowerIntensity = false)
    {
        if (lowerIntensity)
        {
            return userIntensity switch
            {
                Intensity.Light => Intensity.Endurance,
                Intensity.Medium => Intensity.Light,
                Intensity.Heavy => Intensity.Medium,
                _ => throw new NotImplementedException()
            };
        }

        return userIntensity switch
        {
            Intensity.Light => Intensity.Light,
            Intensity.Medium => Intensity.Medium,
            Intensity.Heavy => Intensity.Heavy,
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    ///     Updates the last seen date of the exercise by the user.
    /// </summary>
    /// <param name="refreshAfter">
    ///     When set and the date is > Today, hold off on refreshing the LastSeen date so that we see the same exercises in each workout.
    /// </param>
    public async Task UpdateLastSeenDate(IEnumerable<ExerciseDto> exercises, DateOnly? refreshAfter = null)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var scopedCoreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var exerciseDict = exercises.DistinctBy(e => e.Exercise).ToDictionary(e => e.Exercise);
        foreach (var exercise in exerciseDict.Keys)
        {
            // >= so that today is the last day seeing the same exercises and tomorrow the exercises will refresh.
            if (exerciseDict[exercise].UserExercise!.RefreshAfter == null || Today >= exerciseDict[exercise].UserExercise!.RefreshAfter)
            {
                // If refresh after is today, we want to see a different exercises tomorrow so update the last seen date.
                if (exerciseDict[exercise].UserExercise!.RefreshAfter == null && refreshAfter.HasValue && refreshAfter.Value > Today)
                {
                    exerciseDict[exercise].UserExercise!.RefreshAfter = refreshAfter.Value;
                }
                else
                {
                    exerciseDict[exercise].UserExercise!.RefreshAfter = null;
                    exerciseDict[exercise].UserExercise!.LastSeen = Today;
                }
                scopedCoreContext.UserExercises.Update(exerciseDict[exercise].UserExercise!);
            }
        }

        var exerciseVariationDict = exercises.DistinctBy(e => e.ExerciseVariation).ToDictionary(e => e.ExerciseVariation);
        foreach (var exerciseVariation in exerciseVariationDict.Keys)
        {
            // >= so that today is the last day seeing the same exercises and tomorrow the exercises will refresh.
            if (exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter == null || Today >= exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter)
            {
                // If refresh after is today, we want to see a different exercises tomorrow so update the last seen date.
                if (exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter == null && refreshAfter.HasValue && refreshAfter.Value > Today)
                {
                    exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter = refreshAfter.Value;
                }
                else
                {
                    exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter = null;
                    exerciseVariationDict[exerciseVariation].UserExerciseVariation!.LastSeen = Today;
                }
                scopedCoreContext.UserExerciseVariations.Update(exerciseVariationDict[exerciseVariation].UserExerciseVariation!);
            }
        }

        await scopedCoreContext.SaveChangesAsync();
    }
}
