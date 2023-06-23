using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User;

namespace Web.Components.User;

/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class DeloadViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Deload";

    private readonly Data.Repos.UserRepo _userService;

    public DeloadViewComponent(Data.Repos.UserRepo userService)
    {
        _userService = userService;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var (needsDeload, timeUntilDeload) = await _userService.CheckNewsletterDeloadStatus(user);

        return View("Deload", new DeloadViewModel()
        {
            TimeUntilDeload = timeUntilDeload,
            NeedsDeload = needsDeload
        });
    }
}
