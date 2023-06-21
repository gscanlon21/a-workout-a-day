using Core.Code.Extensions;
using Core.Models.Exercise;
using Core.Models.User;
using Data.Data;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Data.Models.Newsletter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Api.Controllers;

/// <summary>
/// User helpers.
/// </summary>
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private const double WeightSecondaryMusclesXTimesLess = 3;

    private readonly CoreContext _context;

    public UserController(CoreContext context)
    {
        _context = context;
    }

    [AllowAnonymous, HttpGet("token")]
    public ContentResult Token()
    {
        return Content(CreateToken());
    }

    /// <summary>
    /// Grab a user from the db with a specific token
    /// </summary>
    [HttpGet("GetUser")]
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

        IQueryable<User> query = _context.Users.AsSplitQuery().TagWithCallSite();

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

        var user = await query.FirstOrDefaultAsync(u => u.Email == email && u.UserTokens.Any(ut => ut.Token == token));

        if (!allowDemoUser && user?.IsDemoUser == true)
        {
            throw new ArgumentException("User not authorized.", nameof(email));
        }

        return user;
    }

    [HttpGet("CreateToken", Name = "CreateToken")]
    public string CreateToken(int count = 24)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(count));
    }

    private async Task<string> AddUserToken(User user, int durationDays = 2)
    {
        var token = new UserToken(user.Id, CreateToken())
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
    /// ~24 per exercise.
    /// 
    /// https://www.bodybuilding.com/content/how-many-exercises-per-muscle-group.html
    /// 50-70 for minor muscle groups.
    /// 90-120 for major muscle groups.
    /// </summary>
    public static readonly IDictionary<MuscleGroups, Range> MuscleTargets = new Dictionary<MuscleGroups, Range>
    {
        [MuscleGroups.Abdominals] = 100..240, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Obliques] = 100..240, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.ErectorSpinae] = 100..240, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Glutes] = 90..170, // Largest muscle group in the body.
        [MuscleGroups.Hamstrings] = 90..150, // Major muscle.
        [MuscleGroups.Quadriceps] = 90..150, // Major muscle.
        [MuscleGroups.Deltoids] = 90..150, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MuscleGroups.Pectorals] = 90..150, // Major muscle.
        [MuscleGroups.Trapezius] = 90..150, // Major muscle.
        [MuscleGroups.LatissimusDorsi] = 90..150, // Major muscle.
        [MuscleGroups.HipFlexors] = 50..120, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Calves] = 50..120, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Forearms] = 50..120, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Rhomboids] = 50..90, // Minor muscle.
        [MuscleGroups.Biceps] = 50..90, // Minor muscle.
        [MuscleGroups.Triceps] = 50..90, // Minor muscle.
        [MuscleGroups.SerratusAnterior] = 30..70, // Miniature muscle.
        [MuscleGroups.RotatorCuffs] = 30..70, // Miniature muscle.
        [MuscleGroups.HipAdductors] = 30..70, // Miniature muscle.
        [MuscleGroups.TibialisAnterior] = 0..50, // Generally doesn't require strengthening. 
    };

    private async Task<IDictionary<MuscleGroups, int?>> GetWeeklyMuscleVolumeFromMobilityWorkouts(User user, int weeks)
    {
        var mobilityNewsletterGroups = await _context.Newsletters
            .Where(n => n.User.Id == user.Id)
            // Only look at records where the user is not new to fitness.
            .Where(n => n.Date > user.SeasonedDate)
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
            }).AsNoTracking().ToListAsync();

        // .Max/.Min throw exceptions when the collection is empty.
        if (mobilityNewsletterGroups.Any())
        {
            // sa. Drop 4 weeks down to 3.5 weeks if we only have 3.5 weeks of data.
            var actualWeeks = (Today.DayNumber - mobilityNewsletterGroups.Min(n => n.Key).DayNumber) / 7d;
            // User must have at least one week of data before we return anything.
            if (actualWeeks >= 1)
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

                return EnumExtensions.GetSingleValues32<MuscleGroups>()
                    .ToDictionary(m => m, m => (int?)Convert.ToInt32(
                        (monthlyMuscles.Sum(mm => mm.StrengthMuscles.HasFlag(m) ? mm.Volume : 0)
                            // Secondary muscles, count them for less time.
                            // For selecting a workout's exercises, the secondary muscles are valued as half of primary muscles,
                            // ... but here I want them valued less because worked secondary muscles recover faster and don't create as strong of strengthening gains.
                            + monthlyMuscles.Sum(mm => mm.SecondaryMuscles.HasFlag(m) ? mm.Volume : 0) / WeightSecondaryMusclesXTimesLess
                        )
                        / actualWeeks)
                    );
            }
        }

        return EnumExtensions.GetSingleValues32<MuscleGroups>().ToDictionary(m => m, m => (int?)null);
    }

    private async Task<IDictionary<MuscleGroups, int?>> GetWeeklyMuscleVolumeFromStrengthWorkouts(User user, int weeks)
    {
        var strengthNewsletterGroups = await _context.Newsletters
            .Where(n => n.User.Id == user.Id)
            // Only look at records where the user is not new to fitness.
            .Where(n => n.Date > user.SeasonedDate)
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
            }).AsNoTracking().ToListAsync();

        // .Max/.Min throw exceptions when the collection is empty.
        if (strengthNewsletterGroups.Any())
        {
            // sa. Drop 4 weeks down to 3.5 weeks if we only have 3.5 weeks of data.
            var actualWeeks = (Today.DayNumber - strengthNewsletterGroups.Min(n => n.Key).DayNumber) / 7d;
            // User must have at least one week of data before we return anything.
            if (actualWeeks >= 1)
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

                return EnumExtensions.GetSingleValues32<MuscleGroups>()
                    .ToDictionary(m => m, m => (int?)Convert.ToInt32(
                        (monthlyMuscles.Sum(mm => mm.StrengthMuscles.HasFlag(m) ? mm.Volume : 0)
                            // Secondary muscles, count them for less time.
                            // For selecting a workout's exercises, the secondary muscles are valued as half of primary muscles,
                            // ... but here I want them valued less because worked secondary muscles recover faster and don't create as strong of strengthening gains.
                            + monthlyMuscles.Sum(mm => mm.SecondaryMuscles.HasFlag(m) ? mm.Volume : 0) / WeightSecondaryMusclesXTimesLess
                        )
                        / actualWeeks)
                    );
            }
        }

        return EnumExtensions.GetSingleValues32<MuscleGroups>().ToDictionary(m => m, m => (int?)null);
    }

    /// <summary>
    /// Get the user's weekly training volume for each muscle group.
    /// 
    /// Returns `null` when the user is new to fitness.
    /// </summary>
    [HttpGet("GetWeeklyMuscleVolume")]
    public async Task<IDictionary<MuscleGroups, int?>?> GetWeeklyMuscleVolume(User user, int weeks)
    {
        if (weeks < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(weeks));
        }

        if (user.IsNewToFitness || user.Features.HasFlag(Features.Demo))
        {
            // Feature is disabled in the demo.
            // Feature is disabled for users who are new to fitness because they should be more concerned with working out consistently
            // ... and otherwise when you transition from is-new to is-not-new you would get an increased number of accessory exercises
            // ... from trying to try and hit muscle targets for minor muscles that is-new/functional-exercises don't really target.
            return null;
        }

        var weeklyMuscleVolumeFromStrengthWorkouts = await GetWeeklyMuscleVolumeFromStrengthWorkouts(user, weeks);
        var weeklyMuscleVolumeFromMobilityWorkouts = await GetWeeklyMuscleVolumeFromMobilityWorkouts(user, weeks);

        return EnumExtensions.GetSingleValues32<MuscleGroups>().ToDictionary(m => m,
            m =>
            {
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
    [HttpGet("CheckNewsletterDeloadStatus")]
    public async Task<(bool needsDeload, TimeSpan timeUntilDeload)> CheckNewsletterDeloadStatus(User user)
    {
        var lastDeload = await _context.Newsletters.AsNoTracking().TagWithCallSite()
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
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    [HttpGet("GetTodaysNewsletterRotation")]
    public async Task<NewsletterRotation> GetTodaysNewsletterRotation(User user)
    {
        return (await GetCurrentAndUpcomingRotations(user)).First();
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    [HttpGet("GetTodaysNewsletterRotation2")]
    public async Task<NewsletterRotation> GetTodaysNewsletterRotation(User user, Frequency frequency)
    {
        return (await GetCurrentAndUpcomingRotations(user, frequency)).First();
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    [HttpGet("GetTodaysNewsletterRotation3")]
    public async Task<NewsletterTypeGroups> GetCurrentAndUpcomingRotations(User user)
    {
        return await GetCurrentAndUpcomingRotations(user, user.Frequency);
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    [HttpGet("GetCurrentAndUpcomingRotations")]
    public async Task<NewsletterTypeGroups> GetCurrentAndUpcomingRotations(User user, Frequency frequency)
    {
        var previousNewsletter = await _context.Newsletters.AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            // Get the previous newsletter from the same rotation group.
            // So that if a user switches frequencies, they continue where they left off.
            .Where(n => n.Frequency == frequency)
            .OrderByDescending(n => n.Date)
            // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            // Dummy records that are created when the user advances their workout split may also have the same date.
            .ThenByDescending(n => n.Id)
            .FirstOrDefaultAsync();

        return new NewsletterTypeGroups(user, frequency, previousNewsletter?.NewsletterRotation);
    }
}

