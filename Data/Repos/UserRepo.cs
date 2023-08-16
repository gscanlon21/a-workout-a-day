using Core.Code.Extensions;
using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Data.Models.Newsletter;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Data.Repos;

/// <summary>
/// User helpers.
/// </summary>
public class UserRepo
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private const double WeightSecondaryMusclesXTimesLess = 3;

    private readonly CoreContext _context;

    public UserRepo(CoreContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Grab a user from the db with a specific token
    /// </summary>
    public async Task<User?> GetUser(string email, string token,
        bool includeUserEquipments = false,
        bool includeUserExerciseVariations = false,
        bool includeExerciseVariations = false,
        bool includeMuscles = false,
        bool includeFrequencies = false,
        bool allowDemoUser = false)
    {
        IQueryable<User> query = _context.Users.AsSplitQuery().TagWithCallSite();

        if (includeUserEquipments)
        {
            query = query.Include(u => u.UserEquipments);
        }

        if (includeMuscles)
        {
            query = query.Include(u => u.UserMuscleStrengths);
            query = query.Include(u => u.UserMuscleMobilities);
            query = query.Include(u => u.UserMuscleFlexibilities);
        }

        if (includeFrequencies)
        {
            query = query.Include(u => u.UserFrequencies);
        }

        if (includeExerciseVariations)
        {
            query = query.Include(u => u.UserExercises).ThenInclude(ue => ue.Exercise)
                         .Include(u => u.UserVariations).ThenInclude(uv => uv.Variation)
                         .Include(u => u.UserExerciseVariations).ThenInclude(uv => uv.ExerciseVariation);
        }
        else if (includeUserExerciseVariations)
        {
            query = query.Include(u => u.UserExercises).Include(u => u.UserVariations).Include(u => u.UserExerciseVariations);
        }

        var user = await query
            // User token is valid.
            .Where(u => u.UserTokens.Any(ut => ut.Token == token && ut.Expires > DateTime.UtcNow))
            .FirstOrDefaultAsync(u => u.Email == email);

        if (!allowDemoUser && user?.IsDemoUser == true)
        {
            throw new ArgumentException("User not authorized.", nameof(email));
        }

        return user;
    }

    public static string CreateToken(int count = 24)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(count));
    }

    public async Task<string> AddUserToken(User user, DateTime expires)
    {
        var token = new UserToken(user.Id, CreateToken())
        {
            Expires = expires
        };
        user.UserTokens.Add(token);
        await _context.SaveChangesAsync();

        return token.Token;
    }

    public async Task<string> AddUserToken(User user, int durationDays = 1)
    {
        return await AddUserToken(user, DateTime.UtcNow.AddDays(durationDays));
    }

    private async Task<(double weeks, IDictionary<MuscleGroups, int?> volume)> GetWeeklyMuscleVolumeFromMobilityWorkouts(User user, int weeks)
    {
        var mobilityNewsletterGroups = await _context.UserWorkouts
            .Where(n => n.User.Id == user.Id)
            // Only look at records where the user is not new to fitness.
            .Where(n => user.IsNewToFitness || n.Date > user.SeasonedDate)
            // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
            .Where(n => n.UserWorkoutExerciseVariations.Any())
            // Look at mobility workouts only that are within the last X weeks.
            .Where(n => n.Frequency == Frequency.OffDayStretches)
            .Where(n => n.Date >= Today.AddDays(-7 * weeks))
            .GroupBy(n => n.Date)
            .Select(g => new
            {
                g.Key,
                // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created and select first.
                NewsletterVariations = g.OrderByDescending(n => n.Id).First().UserWorkoutExerciseVariations
                    // Only select variations that worked a strengthening intensity.
                    .Where(nv => (Section.Main | Section.Sports).HasFlag(nv.Section))
                    .Select(nv => new
                    {
                        Proficiency = nv.ExerciseVariation.Variation.GetProficiency(nv.Section, g.OrderByDescending(n => n.Id).First().Intensity, g.OrderByDescending(n => n.Id).First().IsDeloadWeek),
                        nv.ExerciseVariation.Variation.StrengthMuscles,
                        nv.ExerciseVariation.Variation.SecondaryMuscles,
                    })
            }).AsNoTracking().ToListAsync();

        // .Max/.Min throw exceptions when the collection is empty.
        if (mobilityNewsletterGroups.Any())
        {
            // sa. Drop 4 weeks down to 3.5 weeks if we only have 3.5 weeks of data.
            var actualWeeks = (Today.DayNumber - mobilityNewsletterGroups.Min(n => n.Key).DayNumber) / 7d;
            // User must have more than one week of data before we return anything.
            if (actualWeeks > UserConsts.MuscleTargetsTakeEffectAfterXWeeks)
            {
                var monthlyMuscles = mobilityNewsletterGroups
                    .SelectMany(ng => ng.NewsletterVariations.Select(nv => new
                    {
                        nv.StrengthMuscles,
                        nv.SecondaryMuscles,
                        // Grabbing the sets based on the current strengthening preference of the user and not the newsletter so that the graph is less misleading.
                        Volume = nv.Proficiency?.Volume ?? 0d
                    }
                    ));

                return (weeks: actualWeeks, volume: EnumExtensions.GetSingleValues32<MuscleGroups>()
                    .ToDictionary(m => m, m => (int?)Convert.ToInt32(
                        (monthlyMuscles.Sum(mm => mm.StrengthMuscles.HasFlag(m) ? mm.Volume : 0)
                            // Secondary muscles, count them for less time.
                            // For selecting a workout's exercises, the secondary muscles are valued as half of primary muscles,
                            // ... but here I want them valued less because worked secondary muscles recover faster and don't create as strong of strengthening gains.
                            + (monthlyMuscles.Sum(mm => mm.SecondaryMuscles.HasFlag(m) ? mm.Volume : 0) / WeightSecondaryMusclesXTimesLess)
                        )
                        / actualWeeks)
                    )
                );
            }
        }

        return (weeks: 0, volume: EnumExtensions.GetSingleValues32<MuscleGroups>().ToDictionary(m => m, m => (int?)null));
    }

    private async Task<(double weeks, IDictionary<MuscleGroups, int?> volume)> GetWeeklyMuscleVolumeFromStrengthWorkouts(User user, int weeks)
    {
        var strengthNewsletterGroups = await _context.UserWorkouts
            .Where(n => n.User.Id == user.Id)
            // Only look at records where the user is not new to fitness.
            .Where(n => user.IsNewToFitness || n.Date > user.SeasonedDate)
            // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
            .Where(n => n.UserWorkoutExerciseVariations.Any())
            // Look at strengthening workouts only that are within the last X weeks.
            .Where(n => n.Frequency != Frequency.OffDayStretches)
            .Where(n => n.Date >= Today.AddDays(-7 * weeks))
            .GroupBy(n => n.Date)
            .Select(g => new
            {
                g.Key,
                // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created and select first.
                NewsletterVariations = g.OrderByDescending(n => n.Id).First().UserWorkoutExerciseVariations
                    // Only select variations that worked a strengthening intensity.
                    .Where(nv => (Section.Main | Section.Sports).HasFlag(nv.Section))
                    .Select(nv => new
                    {
                        Proficiency = nv.ExerciseVariation.Variation.GetProficiency(nv.Section, g.OrderByDescending(n => n.Id).First().Intensity, g.OrderByDescending(n => n.Id).First().IsDeloadWeek),
                        nv.ExerciseVariation.Variation.StrengthMuscles,
                        nv.ExerciseVariation.Variation.SecondaryMuscles,
                    })
            }).AsNoTracking().ToListAsync();

        // .Max/.Min throw exceptions when the collection is empty.
        if (strengthNewsletterGroups.Any())
        {
            // sa. Drop 4 weeks down to 3.5 weeks if we only have 3.5 weeks of data.
            var actualWeeks = (Today.DayNumber - strengthNewsletterGroups.Min(n => n.Key).DayNumber) / 7d;
            // User must have more than one week of data before we return anything.
            if (actualWeeks > UserConsts.MuscleTargetsTakeEffectAfterXWeeks)
            {
                var monthlyMuscles = strengthNewsletterGroups
                    .SelectMany(ng => ng.NewsletterVariations.Select(nv => new
                    {
                        nv.StrengthMuscles,
                        nv.SecondaryMuscles,
                        // Grabbing the sets based on the current strengthening preference of the user and not the newsletter so that the graph is less misleading.
                        Volume = nv.Proficiency?.Volume ?? 0d
                    }
                    ));

                return (weeks: actualWeeks, volume: EnumExtensions.GetSingleValues32<MuscleGroups>()
                    .ToDictionary(m => m, m => (int?)Convert.ToInt32(
                        (monthlyMuscles.Sum(mm => mm.StrengthMuscles.HasFlag(m) ? mm.Volume : 0)
                            // Secondary muscles, count them for less time.
                            // For selecting a workout's exercises, the secondary muscles are valued as half of primary muscles,
                            // ... but here I want them valued less because worked secondary muscles recover faster and don't create as strong of strengthening gains.
                            + (monthlyMuscles.Sum(mm => mm.SecondaryMuscles.HasFlag(m) ? mm.Volume : 0) / WeightSecondaryMusclesXTimesLess)
                        )
                        / actualWeeks)
                    )
                );
            }
        }

        return (weeks: 0, volume: EnumExtensions.GetSingleValues32<MuscleGroups>().ToDictionary(m => m, m => (int?)null));
    }

    /// <summary>
    /// Get the user's weekly training volume for each muscle group.
    /// 
    /// Returns `null` when the user is new to fitness.
    /// </summary>
    public async Task<(double weeks, IDictionary<MuscleGroups, int?>? volume)> GetWeeklyMuscleVolume(User user, int weeks)
    {
        if (weeks < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(weeks));
        }

        if (user.Features.HasFlag(Features.Demo))
        {
            // Feature is disabled in the demo.
            return (weeks: 0, volume: null);
        }

        var (strengthWeeks, weeklyMuscleVolumeFromStrengthWorkouts) = await GetWeeklyMuscleVolumeFromStrengthWorkouts(user, weeks);
        var (_, weeklyMuscleVolumeFromMobilityWorkouts) = await GetWeeklyMuscleVolumeFromMobilityWorkouts(user, weeks);

        return (weeks: strengthWeeks, volume: EnumExtensions.GetSingleValues32<MuscleGroups>().ToDictionary(m => m,
            m =>
            {
                if (weeklyMuscleVolumeFromStrengthWorkouts[m].HasValue && weeklyMuscleVolumeFromMobilityWorkouts[m].HasValue)
                {
                    return weeklyMuscleVolumeFromStrengthWorkouts[m].GetValueOrDefault() + weeklyMuscleVolumeFromMobilityWorkouts[m].GetValueOrDefault();
                }

                return weeklyMuscleVolumeFromStrengthWorkouts[m] ?? weeklyMuscleVolumeFromMobilityWorkouts[m];
            })
        );
    }

    /// <summary>
    /// Checks if the user should deload for a week.
    /// 
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate.
    /// Also to ease up the stress on joints.
    /// </summary>
    public async Task<(bool needsDeload, TimeSpan timeUntilDeload)> CheckNewsletterDeloadStatus(User user)
    {
        var lastDeload = await _context.UserWorkouts.AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            .OrderByDescending(n => n.Date)
            .FirstOrDefaultAsync(n => n.IsDeloadWeek);

        // Grabs the date of Sunday of the current week.
        var currentWeekStart = Today.AddDays(-1 * (int)Today.DayOfWeek);
        // Grabs the Sunday that was the start of the last deload.
        var lastDeloadStartOfWeek = lastDeload != null ? lastDeload.Date.AddDays(-1 * (int)lastDeload.Date.DayOfWeek) : DateOnly.MinValue;
        // Grabs the Sunday at or before the user's created date.
        var createdDateStartOfWeek = user.CreatedDate.AddDays(-1 * (int)user.CreatedDate.DayOfWeek);
        // How far away the last deload need to be before another deload.
        var countupToNextDeload = Today.AddDays(-7 * user.DeloadAfterEveryXWeeks);

        bool isSameWeekAsLastDeload = lastDeload != null && lastDeloadStartOfWeek == currentWeekStart;
        TimeSpan timeUntilDeload = (isSameWeekAsLastDeload, lastDeload) switch
        {
            // There's never been a deload before, calculate the next deload date using the user's created date.
            (false, null) => TimeSpan.FromDays(createdDateStartOfWeek.DayNumber - countupToNextDeload.DayNumber),
            // Calculate the next deload date using the last deload's date.
            (false, not null) => TimeSpan.FromDays(lastDeloadStartOfWeek.DayNumber - countupToNextDeload.DayNumber),
            // Dates are the same week. Keep the deload going until the week is over.
            _ => TimeSpan.Zero
        };

        return (timeUntilDeload <= TimeSpan.Zero, timeUntilDeload);
    }

    /// <summary>
    /// Get the user's current workout.
    /// </summary>
    public async Task<UserWorkout?> GetCurrentWorkout(User user)
    {
        return await _context.UserWorkouts.AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            // Checking the newsletter variations because we create a dummy newsletter to advance the workout split and we want actual workouts.
            .Where(n => n.UserWorkoutExerciseVariations.Any())
            .OrderByDescending(n => n.Date)
            // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            // Dummy records that are created when the user advances their workout split may also have the same date.
            .ThenByDescending(n => n.Id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get the last 7 days of workouts for the user.
    /// </summary>
    public async Task<IList<UserWorkout>> GetPastWorkouts(User user)
    {
        return await _context.UserWorkouts
            .Where(uw => uw.UserId == user.Id)
            // Checking the newsletter variations because we create a dummy newsletter to advance the workout split and we want actual workouts.
            .Where(n => n.UserWorkoutExerciseVariations.Any())
            .OrderByDescending(uw => uw.Date)
            // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            // Dummy records that are created when the user advances their workout split may also have the same date.
            .ThenByDescending(n => n.Id)
            .Skip(1).Take(7)
            .ToListAsync();
    }

    /// <summary>
    /// Calculates the user's next newsletter rotation from the previous newsletter.
    /// 
    /// May return a null rotation when the user has a rest day.
    /// </summary>
    public async Task<(Frequency frequency, WorkoutRotation? rotation)> GetNextRotation(User user)
    {
        var frequency = user.ActualFrequency;
        var rotation = (await GetUpcomingRotations(user, frequency)).FirstOrDefault();
        return (frequency, rotation);
    }

    /// <summary>
    /// Grab the WorkoutRotation[] for the specified frequency.
    /// </summary>
    public async Task<WorkoutSplit> GetUpcomingRotations(User user, Frequency frequency)
    {
        // Not checking for dummy workoutsa here, we want those to apply to alter the next workout rotation.
        var previousNewsletter = await _context.UserWorkouts.AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            // Get the previous newsletter from the same rotation group.
            // So that if a user switches frequencies, they continue where they left off.
            .Where(n => n.Frequency == frequency)
            .OrderByDescending(n => n.Date)
            // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            // Dummy records that are created when the user advances their workout split may also have the same date.
            .ThenByDescending(n => n.Id)
            .FirstOrDefaultAsync();

        return new WorkoutSplit(frequency, user, previousNewsletter?.WorkoutRotation);
    }
}

