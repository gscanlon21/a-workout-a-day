using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.Token;

namespace Web.Components.User;

public class TokenViewComponent(UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Token";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        return View("Token", new TokenViewModel()
        {
            User = user,
            Token = await userRepo.AddUserToken(user, 2)
        });
    }
}
