using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

public class TokenViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Token";

    private readonly Data.Repos.UserRepo _userService;

    public TokenViewComponent(Data.Repos.UserRepo userService)
    {
        _userService = userService;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        return View("Token", new TokenViewModel()
        {
            User = user,
            Token = await _userService.AddUserToken(user, 2)
        });
    }
}
