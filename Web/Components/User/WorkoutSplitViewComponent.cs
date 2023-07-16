using Core.Models.User;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;


/// <summary>
/// Renders an alert box summary of when the suer's next deload week will occur.
/// </summary>
public class WorkoutSplitViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "WorkoutSplit";

    private readonly Data.Repos.UserRepo _userService;

    public WorkoutSplitViewComponent(Data.Repos.UserRepo userService)
    {
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
