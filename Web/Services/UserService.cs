using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Entities.User;

namespace Web.Services;

public class UserService
{
    /// <summary>
    /// Today's date from UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CoreContext _context;

    public UserService(CoreContext context)
    {
        _context = context;
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
            .Where(n => n.User == user)
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

    internal async Task<(bool needsRefresh, TimeSpan timeUntilRefresh)> CheckFunctionalRefreshStatus(User user)
    {
        var lastRefresh = await _context.Newsletters
            .Where(n => n.User == user)
            .OrderBy(n => n.Date)
            .LastOrDefaultAsync(n => n.IsFunctionalRefresh);

        // Grabs the Sunday that was the start of the last deload.
        var lastDeloadStartOfWeek = lastRefresh != null ? lastRefresh.Date : DateOnly.MinValue;
        // Grabs the Sunday at or before the user's created date.
        var createdDateStartOfWeek = user.CreatedDate;
        // How far away the last deload need to be before another deload.
        var countupToNextDeload = Today.AddDays(-1 * user.RefreshFunctionalEveryXDays);

        TimeSpan timeUntilRefresh = (user.Email == User.DemoUser, lastRefresh) switch
        {
            // There's never been a deload before, calculate the next deload date using the user's created date.
            (false, null) => TimeSpan.FromDays(createdDateStartOfWeek.DayNumber - countupToNextDeload.DayNumber),
            // Calculate the next deload date using the last deload's date.
            (false, not null) => TimeSpan.FromDays(lastDeloadStartOfWeek.DayNumber - countupToNextDeload.DayNumber),
            _ => TimeSpan.Zero
        };

        return (timeUntilRefresh <= TimeSpan.Zero, timeUntilRefresh);
    }

    internal async Task<(bool needsRefresh, TimeSpan timeUntilRefresh)> CheckAccessoryRefreshStatus(User user)
    {
        var lastRefresh = await _context.Newsletters
            .Where(n => n.User == user)
            .OrderBy(n => n.Date)
            .LastOrDefaultAsync(n => n.IsAccessoryRefresh);

        // Grabs the Sunday that was the start of the last deload.
        var lastDeloadStartOfWeek = lastRefresh != null ? lastRefresh.Date : DateOnly.MinValue;
        // Grabs the Sunday at or before the user's created date.
        var createdDateStartOfWeek = user.CreatedDate;
        // How far away the last deload need to be before another deload.
        var countupToNextDeload = Today.AddDays(-1 * user.RefreshAccessoryEveryXDays);

        TimeSpan timeUntilRefresh = (user.Email == User.DemoUser, lastRefresh) switch
        {
            // There's never been a deload before, calculate the next deload date using the user's created date.
            (false, null) => TimeSpan.FromDays(createdDateStartOfWeek.DayNumber - countupToNextDeload.DayNumber),
            // Calculate the next deload date using the last deload's date.
            (false, not null) => TimeSpan.FromDays(lastDeloadStartOfWeek.DayNumber - countupToNextDeload.DayNumber),
            _ => TimeSpan.Zero
        };

        return (timeUntilRefresh <= TimeSpan.Zero, timeUntilRefresh);
    }
}

