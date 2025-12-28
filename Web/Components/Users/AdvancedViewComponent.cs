using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.Advanced;

namespace Web.Components.Users;

/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class AdvancedViewComponent : ViewComponent
{
    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "Advanced";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.Users.User user, string token)
    {
        return View("Advanced", new AdvancedViewModel(user, token));
    }
}
