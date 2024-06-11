using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Components.User;

namespace Web.Components.User;

public class WorkoutViewComponent(UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Workout";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        // User has not confirmed their account, they cannot see a workout yet.
        if (!user.LastActive.HasValue)
        {
            return Content("");
        }

        var currentWorkout = await userRepo.GetCurrentWorkoutRotation(user);
        return View("Workout", new WorkoutViewModel()
        {
            User = user,
            Rotation = currentWorkout.Item1,
            Frequency = currentWorkout.Item2,
            Token = await userRepo.AddUserToken(user, durationDays: 1),
        });
    }
}
