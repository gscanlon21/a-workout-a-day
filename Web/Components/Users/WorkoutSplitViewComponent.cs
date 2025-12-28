using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.WorkoutSplit;

namespace Web.Components.Users;

/// <summary>
/// Renders the user's workout split.
/// </summary>
public class WorkoutSplitViewComponent : ViewComponent
{
    private readonly UserRepo _userRepo;

    public WorkoutSplitViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "WorkoutSplit";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.Users.User user, string token)
    {
        return View("WorkoutSplit", new WorkoutSplitViewModel()
        {
            User = user,
            CurrentAndUpcomingRotations = await _userRepo.GetUpcomingRotations(user, user.Frequency)
        });
    }
}
