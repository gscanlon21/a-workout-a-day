using Microsoft.AspNetCore.Mvc;

namespace Web.Components.User;


public class DonateViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Donate";

    public DonateViewComponent() { }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        return View("Donate");
    }
}
