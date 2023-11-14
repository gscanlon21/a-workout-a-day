using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class AdvancedViewComponent(UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Advanced";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var token = await userRepo.AddUserToken(user, durationDays: 1);
        return View("Advanced", new AdvancedViewModel(user, token));
    }
}
