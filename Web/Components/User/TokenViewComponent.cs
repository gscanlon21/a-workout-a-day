using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

public class TokenViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Token";

    private readonly UserRepo _userRepo;

    public TokenViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        return View("Token", new TokenViewModel()
        {
            User = user,
            Token = await _userRepo.AddUserToken(user, 2)
        });
    }
}
