using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.PastWorkout;

namespace Web.Components.User;

public class PastWorkoutViewComponent : ViewComponent
{
    private readonly UserRepo _userRepo;

    public PastWorkoutViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "PastWorkout";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var count = int.TryParse(Request.Query["count"], out int countTmp) ? countTmp : (int?)null;
        var pastWorkouts = await _userRepo.GetPastWorkouts(user, count);
        if (!pastWorkouts.Any())
        {
            return Content("");
        }

        return View("PastWorkout", new PastWorkoutViewModel()
        {
            User = user,
            PastWorkouts = pastWorkouts,
            Token = await _userRepo.AddUserToken(user, durationDays: 1),
        });
    }
}
