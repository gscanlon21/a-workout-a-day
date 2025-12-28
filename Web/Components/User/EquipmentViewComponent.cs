using Core.Models.Equipment;
using Core.Models.User;
using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.Equipment;

namespace Web.Components.User;

/// <summary>
/// Warns the user when they don't have the right equipment for a good workout.
/// </summary>
public class EquipmentViewComponent : ViewComponent
{
    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "Equipment";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.Users.User user, string token)
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

    internal static EquipmentViewModel.UserEquipmentStatus GetUserEquipmentStatus(Data.Entities.Users.User user)
    {
        // User is not seeing strengthening workouts, only seeing mobility workouts. Don't show any message.
        if (user.SendDays == Days.None && user.IncludeMobilityWorkouts)
        {
            return EquipmentViewModel.UserEquipmentStatus.None;
        }

        if (user.Equipment.HasAnyFlag(EquipmentViewModel.ResistanceEquipments))
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
