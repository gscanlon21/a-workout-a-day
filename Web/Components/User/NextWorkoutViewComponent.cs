using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models.User;
using Web.Services;
using Web.ViewModels.User;

namespace Web.Components.User;


/// <summary>
/// Renders an alert box summary of when the suer's next deload week will occur.
/// </summary>
public class NextWorkoutViewComponent : ViewComponent
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "NextWorkout";

    private readonly UserService _userService;
    private readonly CoreContext _context;

    public NextWorkoutViewComponent(CoreContext context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<IViewComponentResult> InvokeAsync(Entities.User.User user)
    {
        DateOnly? nextSendDate = null;
        if (user.RestDays < Days.All)
        {
            nextSendDate = DateTime.UtcNow.Hour <= user.EmailAtUTCOffset ? DateOnly.FromDateTime(DateTime.UtcNow) : DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
            // Next send date is a rest date and user does not want off day workouts, next send date is the day after.
            while ((user.RestDays.HasFlag(DaysExtensions.FromDate(nextSendDate.Value)) && !user.OffDayStretching)
                // User was sent a newsletter for the next send date, next send date is the day after.
                // Checking for variations because we create a dummy newsletter record to advance the workout split.
                || await _context.Newsletters.AnyAsync(n => n.UserId == user.Id && n.NewsletterVariations.Any() && n.Date == nextSendDate.Value))
            {
                nextSendDate = nextSendDate.Value.AddDays(1);
            }
        }

        var nextSendDateTime = nextSendDate?.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(user.EmailAtUTCOffset)));
        var timeUntilNextSend = !nextSendDateTime.HasValue ? null : nextSendDateTime - DateTime.UtcNow;
        return View("NextWorkout", new NextWorkoutViewModel()
        {
            User = user,
            Token = await _userService.AddUserToken(user, durationDays: 2),
            CurrentAndUpcomingRotations = await _userService.GetCurrentAndUpcomingRotations(user),
            TimeUntilNextSend = timeUntilNextSend,
            Today = DaysExtensions.FromDate(Today),
            NextWorkoutSendsToday = timeUntilNextSend.HasValue && DateOnly.FromDateTime(DateTime.UtcNow.Add(timeUntilNextSend.Value)) == Today
        });
    }
}
