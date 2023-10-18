using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

public class PastWorkoutViewComponent(UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "PastWorkout";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var pastWorkouts = await userRepo.GetPastWorkouts(user);
        if (!pastWorkouts.Any())
        {
            return Content("");
        }

        return View("PastWorkout", new PastWorkoutViewModel()
        {
            User = user,
            Token = await userRepo.AddUserToken(user, durationDays: 1),
            PastWorkouts = pastWorkouts,
        });
    }
}
