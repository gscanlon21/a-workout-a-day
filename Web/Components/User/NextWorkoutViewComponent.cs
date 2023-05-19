using Microsoft.AspNetCore.Mvc;
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
    /// For routing
    /// </summary>
    public const string Name = "NextWorkout";

    private readonly UserService _userService;

    public NextWorkoutViewComponent(UserService userService)
    {
        _userService = userService;
    }

    public async Task<IViewComponentResult> InvokeAsync(Entities.User.User user)
    {
        DateOnly? nextSendDate = null;
        if (user.RestDays < Days.All)
        {
            nextSendDate = DateTime.UtcNow.Hour <= user.EmailAtUTCOffset ? DateOnly.FromDateTime(DateTime.UtcNow) : DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
            while (user.RestDays.HasFlag(RestDaysExtensions.FromDate(nextSendDate.Value)))
            {
                nextSendDate = nextSendDate.Value.AddDays(1);
            }
        }

        var nextSendDateTime = nextSendDate?.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(user.EmailAtUTCOffset)));
        return View("NextWorkout", new NextWorkoutViewModel()
        {
            User = user,
            NextWorkoutType = await _userService.GetTodaysNewsletterRotation(user),
            TimeUntilNextSend = !nextSendDateTime.HasValue ? null : nextSendDateTime - DateTime.UtcNow
        });
    }
}
