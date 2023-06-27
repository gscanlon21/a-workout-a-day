using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Data;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Data.Models.Newsletter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Repos;

public partial class NewsletterRepo
{
    /// <summary>
    /// The exercise query runner requires UserExercise/UserExerciseVariation/UserVariation records to have already been made.
    /// There is a small chance for a race-condition if Exercise/ExerciseVariation/Variation records are added after these run in.
    /// I'm not concerned about that possiblity because the data changes infrequently, and the newsletter will resend with the next trigger (twice-hourly).
    /// </summary>
    public async Task AddMissingUserExerciseVariationRecords(Entities.User.User user)
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
    public async Task<UserWorkout> CreateAndAddNewsletterToContext(User user, WorkoutRotation WorkoutRotation, Frequency frequency, bool needsDeload,
        IList<ExerciseModel>? rehabExercises = null,
        IList<ExerciseModel>? warmupExercises = null,
        IList<ExerciseModel>? sportsExercises = null,
        IList<ExerciseModel>? mainExercises = null,
        IList<ExerciseModel>? prehabExercises = null,
        IList<ExerciseModel>? cooldownExercises = null)
    {
        var newsletter = new UserWorkout(Today, user, WorkoutRotation, frequency, isDeloadWeek: needsDeload);
        _context.UserWorkouts.Add(newsletter); // Sets the newsletter.Id after changes are saved.
        await _context.SaveChangesAsync();

        if (rehabExercises != null)
        {
            for (var i = 0; i < rehabExercises.Count; i++)
            {
                var exercise = rehabExercises[i];
                _context.UserWorkoutExerciseVariations.Add(new UserWorkoutExerciseVariation(newsletter, exercise.ExerciseVariation)
                {
                    IntensityLevel = exercise.IntensityLevel,
                    Order = i,
                    Section = Section.Rehab
                });
            }
        }
        if (warmupExercises != null)
        {
            for (var i = 0; i < warmupExercises.Count; i++)
            {
                var exercise = warmupExercises[i];
                _context.UserWorkoutExerciseVariations.Add(new UserWorkoutExerciseVariation(newsletter, exercise.ExerciseVariation)
                {
                    IntensityLevel = exercise.IntensityLevel,
                    Order = i,
                    Section = Section.Warmup
                });
            }
        }
        if (sportsExercises != null)
        {
            for (var i = 0; i < sportsExercises.Count; i++)
            {
                var exercise = sportsExercises[i];
                _context.UserWorkoutExerciseVariations.Add(new UserWorkoutExerciseVariation(newsletter, exercise.ExerciseVariation)
                {
                    IntensityLevel = exercise.IntensityLevel,
                    Order = i,
                    Section = Section.Sports
                });
            }
        }
        if (mainExercises != null)
        {
            for (var i = 0; i < mainExercises.Count; i++)
            {
                var exercise = mainExercises[i];
                _context.UserWorkoutExerciseVariations.Add(new UserWorkoutExerciseVariation(newsletter, exercise.ExerciseVariation)
                {
                    IntensityLevel = exercise.IntensityLevel,
                    Order = i,
                    Section = Section.Main
                });
            }
        }
        if (prehabExercises != null)
        {
            for (var i = 0; i < prehabExercises.Count; i++)
            {
                var exercise = prehabExercises[i];
                _context.UserWorkoutExerciseVariations.Add(new UserWorkoutExerciseVariation(newsletter, exercise.ExerciseVariation)
                {
                    IntensityLevel = exercise.IntensityLevel,
                    Order = i,
                    Section = Section.Prehab
                });
            }
        }
        if (cooldownExercises != null)
        {
            for (var i = 0; i < cooldownExercises.Count; i++)
            {
                var exercise = cooldownExercises[i];
                _context.UserWorkoutExerciseVariations.Add(new UserWorkoutExerciseVariation(newsletter, exercise.ExerciseVariation)
                {
                    IntensityLevel = exercise.IntensityLevel,
                    Order = i,
                    Section = Section.Cooldown
                });
            }
        }

        await _context.SaveChangesAsync();
        return newsletter;
    }

    /// <summary>
    /// 
    /// </summary>
    public IntensityLevel ToIntensityLevel(IntensityLevel userIntensityLevel, bool lowerIntensity = false)
    {
        if (lowerIntensity)
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
    /// <param name="refreshAfter">
    ///     When set and the date is > Today, hold off on refreshing the LastSeen date so that we see the same exercises in each workout.
    /// </param>
    public async Task UpdateLastSeenDate(IEnumerable<ExerciseModel> exercises, DateOnly? refreshAfter = null)
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
