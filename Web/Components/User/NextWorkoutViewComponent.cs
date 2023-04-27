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
        if (user.RestDays < RestDays.All)
        {
            nextSendDate = DateOnly.FromDateTime(DateTime.UtcNow);
            while (user.RestDays.HasFlag(RestDaysExtensions.FromDate(nextSendDate.Value))
            || (!user.RestDays.HasFlag(RestDaysExtensions.FromDate(nextSendDate.Value)) && DateTime.UtcNow.Hour < user.EmailAtUTCOffset))
            {
                nextSendDate = nextSendDate.Value.AddDays(1);
            }
        }

        return View("NextWorkout", new NextWorkoutViewModel()
        {
            TimeUntilNextSend = !nextSendDate.HasValue ? null : nextSendDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(user.EmailAtUTCOffset))) - DateTime.UtcNow
        });
    }
}
