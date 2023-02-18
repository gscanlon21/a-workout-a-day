using Microsoft.AspNetCore.Mvc;
using Web.Services;
using Web.ViewModels.User;

namespace Web.Components.User;

public class RefreshViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Refresh";

    private readonly UserService _userService;

    public RefreshViewComponent(UserService userService)
    {
        _userService = userService;
    }

    public async Task<IViewComponentResult> InvokeAsync(Entities.User.User user)
    {
        var (needsAccessoryRefresh, timeUntilAccessoryRefresh) = await _userService.CheckAccessoryRefreshStatus(user);
        var (needsFunctionalRefresh, timeUntilFunctionalRefresh) = await _userService.CheckFunctionalRefreshStatus(user);

        return View("Refresh", new RefreshViewModel()
        {
            TimeUntilAccessoryRefresh = timeUntilAccessoryRefresh,
            NeedsAccessoryRefresh = needsAccessoryRefresh,
            TimeUntilFunctionalRefresh = timeUntilFunctionalRefresh,
            NeedsFunctionalRefresh = needsFunctionalRefresh
        });
    }
}
