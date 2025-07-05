using Microsoft.AspNetCore.Mvc;

namespace Web.Components.User;

public class AFeastADayViewComponent : ViewComponent
{
    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "AFeastADay";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, string token)
    {
        return View("AFeastADay");
    }
}
