using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Entities.Newsletter;
using Web.Entities.User;
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

