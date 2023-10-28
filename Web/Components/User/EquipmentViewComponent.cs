using Core.Code.Extensions;
using Core.Models.Equipment;
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
        var userEquipmentStatus = GetUserEquipmentStatus(user);
        if (userEquipmentStatus == EquipmentViewModel.UserEquipmentStatus.None)
        {
            return Content("");
        }

        return View("Equipment", new EquipmentViewModel()
        {
            Status = userEquipmentStatus,
        });
    }

    private static EquipmentViewModel.UserEquipmentStatus GetUserEquipmentStatus(Data.Entities.User.User user)
    {
        if (user.Equipment == Equipment.None)
        {
            return EquipmentViewModel.UserEquipmentStatus.MissingEquipment;
        }

        if (!user.Equipment.HasAnyFlag32(EquipmentViewModel.ResistanceEquipments))
        {
            return EquipmentViewModel.UserEquipmentStatus.MissingResistanceEquipment;
        }

        return EquipmentViewModel.UserEquipmentStatus.None;
    }
}
