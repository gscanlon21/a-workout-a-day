using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;


/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class WorkoutSplitViewComponent(UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "WorkoutSplit";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        return View("WorkoutSplit", new WorkoutSplitViewModel()
        {
            User = user,
            CurrentAndUpcomingRotations = await userRepo.GetUpcomingRotations(user, user.Frequency)
        });
    }
}
