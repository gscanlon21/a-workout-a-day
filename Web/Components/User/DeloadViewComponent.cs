using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

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

    private readonly UserRepo _userRepo;

    public DeloadViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var (needsDeload, timeUntilDeload) = await _userRepo.CheckNewsletterDeloadStatus(user);

        return View("Deload", new DeloadViewModel()
        {
            TimeUntilDeload = timeUntilDeload,
            NeedsDeload = needsDeload
        });
    }
}
