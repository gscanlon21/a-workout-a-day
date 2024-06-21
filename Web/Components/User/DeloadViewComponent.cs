using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.Deload;

namespace Web.Components.User;

/// <summary>
/// Renders an alert box summary of when the user's next deload week will occur.
/// </summary>
public class DeloadViewComponent(UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Deload";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        var (needsDeload, timeUntilDeload) = await userRepo.CheckNewsletterDeloadStatus(user);

        return View("Deload", new DeloadViewModel()
        {
            TimeUntilDeload = timeUntilDeload,
            NeedsDeload = needsDeload
        });
    }
}
