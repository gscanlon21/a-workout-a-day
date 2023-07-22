using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

public class WorkoutViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Workout";

    private readonly UserRepo _userRepo;

    public WorkoutViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        // User has not confirmed their account, they cannot see a workout yet.
        if (!user.LastActive.HasValue)
        {
            return Content("");
        }

        var currentWorkout = await _userRepo.GetCurrentWorkout(user);
        if (currentWorkout == null)
        {
            return Content("");
        }

        return View("Workout", new WorkoutViewModel()
        {
            User = user,
            CurrentWorkout = currentWorkout,
            Token = await _userRepo.AddUserToken(user, durationDays: 1),
        });
    }
}
