using Core.Models.Equipment;

namespace Web.ViewModels.User.Components;

public class EquipmentViewModel
{
    public required UserEquipmentStatus Status { get; init; }

    public enum UserEquipmentStatus
    {
        None = 0,
        MissingEquipment = 1,
        MissingResistanceEquipment = 2,
    }

    public const Equipment ResistanceEquipments =
        Equipment.ResistanceBands
        | Equipment.GymnasticRings
        | Equipment.Dumbbells
        | Equipment.Barbell
        | Equipment.Kettlebells
        | Equipment.TRXSystem;
}
