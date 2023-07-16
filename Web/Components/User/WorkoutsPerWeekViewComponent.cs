using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

/// <summary>
/// Renders an alert box summary of how many workouts the user is working per week.
/// </summary>
public class WorkoutsPerWeekViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "WorkoutsPerWeek";

    public WorkoutsPerWeekViewComponent() { }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        if (user == null)
        {
            return Content(string.Empty);
        }

        return View("WorkoutsPerWeek", new WorkoutsPerWeekViewModel()
        {
            User = user,
        });
    }
}
