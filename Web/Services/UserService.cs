using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        bool includeVariations = false,
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

        if (includeVariations)
        {
            query = query.Include(u => u.UserExercises).ThenInclude(ue => ue.Exercise).Include(u => u.UserVariations).ThenInclude(uv => uv.Variation);
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

    /// <summary>
    /// The range of time under tension each muscle group should be exposed to each week.
    /// </summary>
    public static readonly IDictionary<MuscleGroups, Range> MuscleTargets = new Dictionary<MuscleGroups, Range>
    {
        [MuscleGroups.Abdominals] = 250..500, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Obliques] = 250..500, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.ErectorSpinae] = 250..500, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Glutes] = 250..500, // Largest muscle group in the body.
        [MuscleGroups.Hamstrings] = 200..400, // Major muscle.
        [MuscleGroups.Quadriceps] = 200..400, // Major muscle.
        [MuscleGroups.LatissimusDorsi] = 200..400, // Major muscle.
        [MuscleGroups.Pectorals] = 200..400, // Major muscle.
        [MuscleGroups.Trapezius] = 200..400, // Major muscle.
        [MuscleGroups.Rhomboids] = 100..300, // Minor muscle.
        [MuscleGroups.Deltoids] = 100..300, // Minor muscle.
        [MuscleGroups.Biceps] = 100..300, // Minor muscle.
        [MuscleGroups.Triceps] = 100..300, // Minor muscle.
        [MuscleGroups.Forearms] = 100..300, // Minor muscle.
        [MuscleGroups.Calves] = 100..300, // Minor muscle.
        [MuscleGroups.HipFlexors] = 100..300, // Minor muscle.
        [MuscleGroups.HipAdductors] = 100..300, // Minor muscle.
        [MuscleGroups.SerratusAnterior] = 50..250, // Miniature muscle.
        [MuscleGroups.RotatorCuffs] = 50..250, // Miniature muscle.
        [MuscleGroups.TibialisAnterior] = 0..200, // Generally doesn't require strengthening. 
    };

    internal async Task<IDictionary<MuscleGroups, int>?> GetWeeklyMuscleVolume(User user, int avgOverXWeeks, bool includeNewToFitness = false)
    {
        if (avgOverXWeeks < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(avgOverXWeeks));
        }

        var sendDaysXWeeks = avgOverXWeeks * BitOperations.PopCount((ulong)user.SendDays);
        var newsletters = await _context.Newsletters.AsNoTracking()
            .Where(n => n.User.Id == user.Id)
            .Where(n => includeNewToFitness || !n.IsNewToFitness)
            // Check the same Frequency because that changes the workouts
            .Where(n => n.Frequency == user.Frequency)
            // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
            .Where(n => n.NewsletterVariations.Any())
            // IntensityLevel does not change the workouts, commenting that out. All variations have all strength intensities.
            //.Where(n => n.IntensityLevel == user.IntensityLevel)
            .OrderByDescending(n => n.Date)
            // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created.
            .ThenByDescending(n => n.Id)
            .Take(sendDaysXWeeks)
            .Select(newsletter => newsletter.NewsletterVariations
                // Only select variations that worked a strengthening intensity.
                .Where(newsletterVariation => newsletterVariation.IntensityLevel == IntensityLevel.Light
                    || newsletterVariation.IntensityLevel == IntensityLevel.Medium
                    || newsletterVariation.IntensityLevel == IntensityLevel.Heavy
                    || newsletterVariation.IntensityLevel == IntensityLevel.Endurance
                )
                .Select(newsletterVariation => new
                {
                    newsletterVariation.Variation.StrengthMuscles,
                    newsletterVariation.Variation.SecondaryMuscles,
                    newsletterVariation.Variation.Intensities.First(i => i.IntensityLevel == newsletterVariation.IntensityLevel).Proficiency
                })
            ).ToListAsync();

        avgOverXWeeks = newsletters.Count(n => n.Any()) / BitOperations.PopCount((ulong)user.SendDays);
        sendDaysXWeeks = avgOverXWeeks * BitOperations.PopCount((ulong)user.SendDays);
        if (avgOverXWeeks >= 1)
        {
            newsletters = newsletters.Where(n => n.Any()).Take(sendDaysXWeeks).ToList();
            var monthlyMuscles = newsletters.SelectMany(n => n.Select(nv => new
            {
                nv.StrengthMuscles,
                nv.SecondaryMuscles,
                // Grabbing the sets based on the current strengthening preference of the user and not the newsletter so that the graph is less misleading.
                TimeUnderTension = nv.Proficiency?.TimeUnderTension ?? 0d
            }));

            return EnumExtensions.GetSingleValues32<MuscleGroups>()
                .ToDictionary(m => m, m => Convert.ToInt32(
                    (monthlyMuscles.Sum(mm => mm.StrengthMuscles.HasFlag(m) ? mm.TimeUnderTension : 0)
                        // Secondary muscles, count them for half time.
                        + (monthlyMuscles.Sum(mm => mm.SecondaryMuscles.HasFlag(m) ? mm.TimeUnderTension : 0) / 2)
                    )
                    / avgOverXWeeks)
                );
        }

        return null;
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
    internal async Task<NewsletterRotation> GetTodaysNewsletterRotation(int userId, Frequency frequency)
    {
        return (await GetCurrentAndUpcomingRotations(userId, frequency)).First();
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    internal async Task<NewsletterTypeGroups> GetCurrentAndUpcomingRotations(User user)
    {
        return await GetCurrentAndUpcomingRotations(user.Id, user.Frequency);
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    internal async Task<NewsletterTypeGroups> GetCurrentAndUpcomingRotations(int userId, Frequency frequency)
    {
        var previousNewsletter = await _context.Newsletters
            .Where(n => n.UserId == userId)
            // Get the previous newsletter from the same rotation group.
            // So that if a user switches frequencies, they continue where they left off.
            .Where(n => n.Frequency == frequency)
            .OrderBy(n => n.Date)
            // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            // Dummy records that are created when the user advances their workout split may also have the same date.
            .ThenBy(n => n.Id) 
            .LastOrDefaultAsync();

        return new NewsletterTypeGroups(frequency, previousNewsletter?.NewsletterRotation);
    }
}

