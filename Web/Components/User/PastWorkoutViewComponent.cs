using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.PastWorkout;

namespace Web.Components.User;

public class PastWorkoutViewComponent(UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "PastWorkout";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var count = int.TryParse(Request.Query["count"], out int countTmp) ? countTmp : (int?)null;
        var pastWorkouts = await userRepo.GetPastWorkouts(user, count);
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
