using Data;
using Data.Entities.User;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class AdvancedViewComponent(CoreContext coreContext, UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Advanced";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var token = await userRepo.AddUserToken(user, durationDays: 1);
        var userPreference = (await coreContext.UserPreferences.FirstOrDefaultAsync(up => up.User == user)) ?? new UserPreference(user);
        
        return View("Advanced", new AdvancedViewModel(user, userPreference, token));
    }
}
