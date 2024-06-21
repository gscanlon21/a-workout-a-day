using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.Confirmation;

namespace Web.Components.User;

/// <summary>
/// Renders an alert box summary whether the user is confirmed or not.
/// </summary>
public class ConfirmationViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Confirmation";

    public ConfirmationViewComponent() { }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        return View("Confirmation", new ConfirmationViewModel()
        {
            User = user,
        });
    }
}
