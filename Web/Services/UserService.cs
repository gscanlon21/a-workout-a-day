using Microsoft.EntityFrameworkCore;
using System.Numerics;
using Web.Code.Extensions;
using Web.Data;
using Web.Entities.Newsletter;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;

namespace Web.Services;

/// <summary>
/// User helpers.
/// </summary>
public class UserService
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private const int WeighSecondaryMusclesXTimesLess = 3;

    private readonly CoreContext _context;

    public UserService(CoreContext context)
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
        if (_context.Users == null)
        {
            return null;
        }

        IQueryable<User> query = _context.Users.AsSplitQuery();

        if (includeUserEquipments)
        {
            query = query.Include(u => u.UserEquipments);
        }

        if (includeMuscles)
        {
            query = query.Include(u => u.UserMuscles);
        }

        if (includeFrequencies)
        {
            query = query.Include(u => u.UserFrequencies);
        }

        if (includeExerciseVariations)
        {
            query = query.Include(u => u.UserExercises).ThenInclude(ue => ue.Exercise)
                         .Include(u => u.UserVariations).ThenInclude(uv => uv.Variation);
        }
        else if (includeUserExerciseVariations)
        {
            query = query.Include(u => u.UserExercises).Include(u => u.UserVariations);
        }

        var user = await query.FirstOrDefaultAsync(u => u.Email == email && (u.UserTokens.Any(ut => ut.Token == token)));

        if (!allowDemoUser && user?.IsDemoUser == true)
        {
            throw new ArgumentException("User not authorized.", nameof(email));
        }

        return user;
    }

    public async Task<string> AddUserToken(User user, int durationDays = 2)
    {
        var token = new UserToken(user.Id)
        {
            Expires = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(durationDays)
        };
        user.UserTokens.Add(token);
        await _context.SaveChangesAsync();

        return token.Token;
    }

    public const int IncrementMuscleTargetBy = 10;

    /// <summary>
    /// The volume each muscle group should be exposed to each week.
    /// 
    /// https://www.bodybuilding.com/content/how-many-exercises-per-muscle-group.html
    /// 50-70 for minor muscle groups.
    /// 90-120 for major muscle groups.
    /// </summary>
    public static readonly IDictionary<MuscleGroups, Range> MuscleTargets = new Dictionary<MuscleGroups, Range>
    {
        [MuscleGroups.Abdominals] = 100..250, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Obliques] = 100..250, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.ErectorSpinae] = 100..250, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Glutes] = 100..200, // Largest muscle group in the body.
        [MuscleGroups.Hamstrings] = 90..150, // Major muscle.
        [MuscleGroups.Quadriceps] = 90..150, // Major muscle.
        [MuscleGroups.LatissimusDorsi] = 90..150, // Major muscle.
        [MuscleGroups.Pectorals] = 90..150, // Major muscle.
        [MuscleGroups.Trapezius] = 90..150, // Major muscle.
        [MuscleGroups.HipFlexors] = 50..130, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Rhomboids] = 50..90, // Minor muscle.
        [MuscleGroups.Deltoids] = 50..90, // Minor muscle.
        [MuscleGroups.Biceps] = 50..90, // Minor muscle.
        [MuscleGroups.Triceps] = 50..90, // Minor muscle.
        [MuscleGroups.Forearms] = 50..90, // Minor muscle.
        [MuscleGroups.Calves] = 50..90, // Minor muscle.
        [MuscleGroups.HipAdductors] = 50..90, // Minor muscle.
        [MuscleGroups.SerratusAnterior] = 20..80, // Miniature muscle.
        [MuscleGroups.RotatorCuffs] = 20..80, // Miniature muscle.
        [MuscleGroups.TibialisAnterior] = 0..50, // Generally doesn't require strengthening. 
    };

    private async Task<IDictionary<MuscleGroups, int?>> GetWeeklyMuscleVolumeFromMobilityWorkouts(User user, int weeks)
    {
        var mobilityWorkoutsPerWeek = BitOperations.PopCount((ulong)user.RestDays);
        if (user.OffDayStretching && mobilityWorkoutsPerWeek >= 1 && !user.IsNewToFitness)
        {
            var mobilityNewsletterGroups = await _context.Newsletters.AsNoTracking()
                .Where(n => n.User.Id == user.Id)
                // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
                .Where(n => n.NewsletterExerciseVariations.Any())
                // Look at mobility workouts only that are within the last X weeks.
                .Where(n => n.Frequency == Frequency.OffDayStretches)
                .Where(n => n.Date >= Today.AddDays(-7 * weeks))
                .GroupBy(n => n.Date)
                .Select(g => new
                {
                    g.Key,
                    // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created and select first.
                    NewsletterVariations = g.OrderByDescending(n => n.Id).First().NewsletterExerciseVariations
                        // Only select variations that worked a strengthening intensity.
                        .Where(newsletterVariation => newsletterVariation.IntensityLevel == IntensityLevel.Light
                            || newsletterVariation.IntensityLevel == IntensityLevel.Medium
                            || newsletterVariation.IntensityLevel == IntensityLevel.Heavy
                            || newsletterVariation.IntensityLevel == IntensityLevel.Endurance
                        )
                        .Select(newsletterVariation => new
                        {
                            newsletterVariation.ExerciseVariation.Variation.StrengthMuscles,
                            newsletterVariation.ExerciseVariation.Variation.SecondaryMuscles,
                            newsletterVariation.ExerciseVariation.Variation.Intensities.First(i => i.IntensityLevel == newsletterVariation.IntensityLevel).Proficiency
                        })
                }).ToListAsync();

            // sa. Drop 3.5 weeks down to 3 weeks.
            weeks = mobilityNewsletterGroups.Count / mobilityWorkoutsPerWeek;
            // User must have at least one week of data before we return anything.
            if (weeks >= 1)
            {
                var monthlyMuscles = mobilityNewsletterGroups
                    // If we have 3.5 weeks of data, drop that extra half-week.
                    .OrderByDescending(n => n.Key).Take(weeks * mobilityWorkoutsPerWeek)
                    .SelectMany(ng => ng.NewsletterVariations.Select(nv => new
                    {
                        nv.StrengthMuscles,
                        nv.SecondaryMuscles,
                        // Grabbing the sets based on the current strengthening preference of the user and not the newsletter so that the graph is less misleading.
                        Volume = nv.Proficiency?.Volume ?? 0d
                    }
                    ));

                return EnumExtensions.GetSingleValues32<MuscleGroups>()
                    .ToDictionary(m => m, m => (int?)Convert.ToInt32(
                        (monthlyMuscles.Sum(mm => mm.StrengthMuscles.HasFlag(m) ? mm.Volume : 0)
                            // Secondary muscles, count them for less time.
                            // For selecting a workout's exercises, the secondary muscles are valued as half of primary muscles,
                            // ... but here I want them valued less because worked secondary muscles recover faster and don't create as strong of strengthening gains.
                            + (monthlyMuscles.Sum(mm => mm.SecondaryMuscles.HasFlag(m) ? mm.Volume : 0) / WeighSecondaryMusclesXTimesLess)
                        )
                        / weeks)
                    );
            }
        }

        return EnumExtensions.GetSingleValues32<MuscleGroups>().ToDictionary(m => m, m => (int?)null);
    }

    private async Task<IDictionary<MuscleGroups, int?>> GetWeeklyMuscleVolumeFromStrengthWorkouts(User user, int weeks)
    {
        var strengthWorkoutsPerWeek = BitOperations.PopCount((ulong)user.SendDays);
        if (strengthWorkoutsPerWeek >= 1 && !user.IsNewToFitness)
        {
            var strengthNewsletterGroups = await _context.Newsletters.AsNoTracking()
                .Where(n => n.User.Id == user.Id)
                // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
                .Where(n => n.NewsletterExerciseVariations.Any())
                // Look at strengthening workouts only that are within the last X weeks.
                .Where(n => n.Frequency != Frequency.OffDayStretches)
                .Where(n => n.Date >= Today.AddDays(-7 * weeks))
                .GroupBy(n => n.Date)
                .Select(g => new
                {
                    g.Key,
                    // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created and select first.
                    NewsletterVariations = g.OrderByDescending(n => n.Id).First().NewsletterExerciseVariations
                        // Only select variations that worked a strengthening intensity.
                        .Where(newsletterVariation => newsletterVariation.IntensityLevel == IntensityLevel.Light
                            || newsletterVariation.IntensityLevel == IntensityLevel.Medium
                            || newsletterVariation.IntensityLevel == IntensityLevel.Heavy
                            || newsletterVariation.IntensityLevel == IntensityLevel.Endurance
                        )
                        .Select(newsletterVariation => new
                        {
                            newsletterVariation.ExerciseVariation.Variation.StrengthMuscles,
                            newsletterVariation.ExerciseVariation.Variation.SecondaryMuscles,
                            newsletterVariation.ExerciseVariation.Variation.Intensities.First(i => i.IntensityLevel == newsletterVariation.IntensityLevel).Proficiency
                        })
                }).ToListAsync();

            // sa. Drop 3.5 weeks down to 3 weeks.
            weeks = strengthNewsletterGroups.Count / strengthWorkoutsPerWeek;
            // User must have at least one week of data before we return anything.
            if (weeks >= 1)
            {
                var monthlyMuscles = strengthNewsletterGroups
                    // If we have 3.5 weeks of data, drop that extra half-week.
                    .OrderByDescending(n => n.Key).Take(weeks * strengthWorkoutsPerWeek)
                    .SelectMany(ng => ng.NewsletterVariations.Select(nv => new
                    {
                        nv.StrengthMuscles,
                        nv.SecondaryMuscles,
                        // Grabbing the sets based on the current strengthening preference of the user and not the newsletter so that the graph is less misleading.
                        Volume = nv.Proficiency?.Volume ?? 0d
                    }
                    ));

                return EnumExtensions.GetSingleValues32<MuscleGroups>()
                    .ToDictionary(m => m, m => (int?)Convert.ToInt32(
                        (monthlyMuscles.Sum(mm => mm.StrengthMuscles.HasFlag(m) ? mm.Volume : 0)
                            // Secondary muscles, count them for less time.
                            // For selecting a workout's exercises, the secondary muscles are valued as half of primary muscles,
                            // ... but here I want them valued less because worked secondary muscles recover faster and don't create as strong of strengthening gains.
                            + (monthlyMuscles.Sum(mm => mm.SecondaryMuscles.HasFlag(m) ? mm.Volume : 0) / WeighSecondaryMusclesXTimesLess)
                        )
                        / weeks)
                    );
            }
        }

        return EnumExtensions.GetSingleValues32<MuscleGroups>().ToDictionary(m => m, m => (int?)null);
    }

    /// <summary>
    /// Get the user's weekly training volume for each muscle group.
    /// </summary>
    internal async Task<IDictionary<MuscleGroups, int?>?> GetWeeklyMuscleVolume(User user, int weeks)
    {
        if (weeks < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(weeks));
        }

        var weeklyMuscleVolumeFromStrengthWorkouts = await GetWeeklyMuscleVolumeFromStrengthWorkouts(user, weeks);
        var weeklyMuscleVolumeFromMobilityWorkouts = await GetWeeklyMuscleVolumeFromMobilityWorkouts(user, weeks);

        return EnumExtensions.GetSingleValues32<MuscleGroups>().ToDictionary(m => m, 
            m => { 
                if (weeklyMuscleVolumeFromStrengthWorkouts[m].HasValue && weeklyMuscleVolumeFromMobilityWorkouts[m].HasValue)
                {
                    return weeklyMuscleVolumeFromStrengthWorkouts[m].GetValueOrDefault() + weeklyMuscleVolumeFromMobilityWorkouts[m].GetValueOrDefault();
                }

                return weeklyMuscleVolumeFromStrengthWorkouts[m] ?? weeklyMuscleVolumeFromMobilityWorkouts[m];
            });
    }

    /// <summary>
    /// Checks if the user should deload for a week.
    /// 
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate.
    /// Also to ease up the stress on joints.
    /// </summary>
    internal async Task<(bool needsDeload, TimeSpan timeUntilDeload)> CheckNewsletterDeloadStatus(User user)
    {
        var lastDeload = await _context.Newsletters
            .Where(n => n.UserId == user.Id)
            .OrderBy(n => n.Date)
            .LastOrDefaultAsync(n => n.IsDeloadWeek);

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
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    internal async Task<NewsletterRotation> GetTodaysNewsletterRotation(User user)
    {
        return (await GetCurrentAndUpcomingRotations(user)).First();
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    internal async Task<NewsletterRotation> GetTodaysNewsletterRotation(User user, Frequency frequency)
    {
        return (await GetCurrentAndUpcomingRotations(user, frequency)).First();
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    internal async Task<NewsletterTypeGroups> GetCurrentAndUpcomingRotations(User user)
    {
        return await GetCurrentAndUpcomingRotations(user, user.Frequency);
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    internal async Task<NewsletterTypeGroups> GetCurrentAndUpcomingRotations(User user, Frequency frequency)
    {
        var previousNewsletter = await _context.Newsletters
            .Where(n => n.UserId == user.Id)
            // Get the previous newsletter from the same rotation group.
            // So that if a user switches frequencies, they continue where they left off.
            .Where(n => n.Frequency == frequency)
            .OrderBy(n => n.Date)
            // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            // Dummy records that are created when the user advances their workout split may also have the same date.
            .ThenBy(n => n.Id)
            .LastOrDefaultAsync();

        return new NewsletterTypeGroups(user, user.Frequency, previousNewsletter?.NewsletterRotation);
    }
}

