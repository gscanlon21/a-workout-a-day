using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.PastWorkouts;

namespace Web.Components.Users;

public class PastWorkoutsViewComponent : ViewComponent
{
    private readonly UserRepo _userRepo;

    public PastWorkoutsViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "PastWorkouts";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.Users.User user, string token)
    {
        // User has not confirmed their account, newsletters won't render.
        if (!user.LastActive.HasValue)
        {
            return Content("");
        }

        var count = int.TryParse(Request.Query["count"], out int countTmp) ? countTmp : (int?)null;
        var pastWorkouts = await _userRepo.GetPastWorkouts(user, count);
        if (!pastWorkouts.Any())
        {
            return Content("");
        }

        return View("PastWorkouts", new PastWorkoutsViewModel()
        {
            User = user,
            Token = token,
            PastWorkouts = pastWorkouts,
        });
    }
}
