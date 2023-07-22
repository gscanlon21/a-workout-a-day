using Data.Repos;
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

    private readonly UserRepo _userRepo;

    public WorkoutSplitViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        return View("WorkoutSplit", new WorkoutSplitViewModel()
        {
            User = user,
            CurrentAndUpcomingRotations = await _userRepo.GetUpcomingRotations(user, user.Frequency)
        });
    }
}
