using Microsoft.AspNetCore.Mvc;
using Web.Data;
using Web.ViewModels.User;

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

    private readonly CoreContext _context;

    public WorkoutsPerWeekViewComponent(CoreContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(Entities.User.User user)
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
