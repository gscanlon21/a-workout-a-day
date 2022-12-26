using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using Web.Data;
using Web.Entities.User;
using Web.Extensions;
using Web.Models.Exercise;
using Web.ViewModels.User;

namespace Web.Components;

public class DeloadViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Deload";

    /// <summary>
    /// Today's date from UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CoreContext _context;

    public DeloadViewComponent(CoreContext context)
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

        bool needsDeload =
            // Dates are the same week. Keep the deload going until the week is over.
            (lastDeload != null && lastDeload.Date.AddDays(-1 * (int)lastDeload.Date.DayOfWeek) == Today.AddDays(-1 * (int)Today.DayOfWeek))
            // Or the last deload was 1+ months ago.
            || (lastDeload != null && lastDeload.Date <= Today.AddDays(-7 * user.DeloadAfterEveryXWeeks))
            // Or there has never been a deload before, look at the user's created date.
            || (lastDeload == null && user.CreatedDate <= Today.AddDays(-7 * user.DeloadAfterEveryXWeeks));

        TimeSpan timeUntilDeload = (needsDeload, lastDeload) switch
        {
            // There's never been a deload before, calculate the next deload date using the user's created date.
            (false, null) => TimeSpan.FromDays(user.CreatedDate.DayNumber - Today.AddDays(-7 * user.DeloadAfterEveryXWeeks).DayNumber),
            // Calculate the next deload date using the last deload's date.
            (false, not null) => TimeSpan.FromDays(lastDeload.Date.DayNumber - Today.AddDays(-7 * user.DeloadAfterEveryXWeeks).DayNumber),
            _ => TimeSpan.Zero
        };

        return (needsDeload, timeUntilDeload);
    }

    public async Task<IViewComponentResult> InvokeAsync(User user)
    {
        var deloadStatus = await CheckNewsletterDeloadStatus(user);
        return View("Deload", new DeloadViewModel()
        {
            TimeUntilDeload = deloadStatus.timeUntilDeload,
            NeedsDeload = deloadStatus.needsDeload
        });
    }
}
