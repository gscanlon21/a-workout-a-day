using Core.Models.User;
using Data.Data;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User;

namespace Web.Components.User;



/// <summary>
/// Renders an alert box summary of when the suer's next deload week will occur.
/// </summary>
public class WorkoutSplitViewComponent : ViewComponent
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "WorkoutSplit";

    private readonly Data.Repos.UserRepo _userService;
    private readonly CoreContext _context;

    public WorkoutSplitViewComponent(CoreContext context, Data.Repos.UserRepo userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        if (user.Frequency != Frequency.Custom)
        {
            return Content(string.Empty);
        }

        return View("WorkoutSplit", new WorkoutSplitViewModel()
        {
            CurrentAndUpcomingRotations = await _userService.GetCurrentAndUpcomingRotations(user)
        });
    }
}
