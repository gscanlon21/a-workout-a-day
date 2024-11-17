using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.WorkoutsPerWeek;

namespace Web.Components.User;

/// <summary>
/// Renders an alert box summary of how many workouts the user is working per week.
/// </summary>
public class WorkoutsPerWeekViewComponent : ViewComponent
{
    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "WorkoutsPerWeek";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, string token)
    {
        if (user == null)
        {
            return Content(string.Empty);
        }

        return View("WorkoutsPerWeek", new WorkoutsPerWeekViewModel(user));
    }
}
