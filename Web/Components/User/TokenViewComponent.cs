using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User;

namespace Web.Components.User;

public class TokenViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Token";

    private readonly Web.Services.UserService _userService;

    public TokenViewComponent(Web.Services.UserService userService)
    {
        _userService = userService;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        return Content("");

        return View("Token", new TokenViewModel()
        {
        });
    }
}
