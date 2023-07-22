using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

public class PastWorkoutViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "PastWorkout";

    private readonly UserRepo _userRepo;

    public PastWorkoutViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var pastWorkouts = await _userRepo.GetPastWorkouts(user);
        if (!pastWorkouts.Any())
        {
            return Content("");
        }

        return View("PastWorkout", new PastWorkoutViewModel()
        {
            User = user,
            Token = await _userRepo.AddUserToken(user, durationDays: 1),
            PastWorkouts = pastWorkouts,
        });
    }
}
