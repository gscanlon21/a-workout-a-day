using Microsoft.AspNetCore.Mvc;
using Web.Services;
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

    private readonly UserService _userService;

    public DeloadViewComponent(UserService userService)
    {
        _userService = userService;
    }

    public async Task<IViewComponentResult> InvokeAsync(Entities.User.User user)
    {
        var (needsDeload, timeUntilDeload) = await _userService.CheckNewsletterDeloadStatus(user);

        return View("Deload", new DeloadViewModel()
        {
            TimeUntilDeload = timeUntilDeload,
            NeedsDeload = needsDeload
        });
    }
}
