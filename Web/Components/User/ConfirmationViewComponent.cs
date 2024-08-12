using Data.Repos;
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

    private readonly UserRepo _userRepo;

    public ConfirmationViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        // User has already confirmed their account.
        if (user.LastActive.HasValue)
        {
            return Content("");
        }

        return View("Confirmation", new ConfirmationViewModel()
        {
            User = user,
            Token = await _userRepo.AddUserToken(user)
        });
    }
}
