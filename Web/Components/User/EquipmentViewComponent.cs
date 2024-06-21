using Core.Code.Extensions;
using Core.Models.Equipment;
using Core.Models.User;
using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.Equipment;

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

    internal static EquipmentViewModel.UserEquipmentStatus GetUserEquipmentStatus(Data.Entities.User.User user)
    {
        // User is not seeing strengthening workouts, only seeing mobility workouts. Don't show any message.
        if (user.SendDays == Days.None && user.IncludeMobilityWorkouts)
        {
            return EquipmentViewModel.UserEquipmentStatus.None;
        }

        if (user.Equipment.HasAnyFlag32(EquipmentViewModel.ResistanceEquipments))
        {
            return EquipmentViewModel.UserEquipmentStatus.None;
        }

        if (user.Equipment != Equipment.None)
        {
            return EquipmentViewModel.UserEquipmentStatus.MissingResistanceEquipment;
        }

        return EquipmentViewModel.UserEquipmentStatus.MissingEquipment;
    }
}
