using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;


/// <summary>
/// Renders an alert box summary of when the suer's next deload week will occur.
/// </summary>
public class EquipmentViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Equipment";

    public EquipmentViewComponent() { }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        return View("Equipment", new EquipmentViewModel()
        {
            UserHasEquipment = user.EquipmentIds.Any(),
        });
    }
}
