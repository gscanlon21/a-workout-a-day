using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.Deload;

namespace Web.Components.User;

/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class DeloadViewComponent : ViewComponent
{
    private readonly UserRepo _userRepo;

    public DeloadViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "Deload";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.Users.User user, string token)
    {
        var (needsDeload, timeUntilDeload) = await _userRepo.CheckNewsletterDeloadStatus(user);

        return View("Deload", new DeloadViewModel()
        {
            NeedsDeload = needsDeload,
            TimeUntilDeload = timeUntilDeload,
        });
    }
}
