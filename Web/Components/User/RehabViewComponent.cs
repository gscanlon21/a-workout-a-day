using Core.Models.Exercise;
using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.Rehab;

namespace Web.Components.User;

/// <summary>
/// Displays a warning when the user has a Rehab Focus group selected, 
/// since that causes the workouts to exclude that muscle group.
/// </summary>
public class RehabViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Rehab";

    public RehabViewComponent() { }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        if (user.RehabFocus == RehabFocus.None)
        {
            return Content("");
        }

        return View("Rehab", new RehabViewModel()
        {
            RehabFocus = user.RehabFocus
        });
    }
}
