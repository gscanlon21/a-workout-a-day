
namespace Web.Views.Shared.Components.Equipment;

public class EquipmentViewModel
{
    public required UserEquipmentStatus Status { get; init; }

    public enum UserEquipmentStatus
    {
        None = 0,
        MissingEquipment = 1,
        MissingResistanceEquipment = 2,
    }

    public const Core.Models.Equipment.Equipment ResistanceEquipments =
        Core.Models.Equipment.Equipment.ResistanceBands
        | Core.Models.Equipment.Equipment.GymnasticRings
        | Core.Models.Equipment.Equipment.Dumbbells
        | Core.Models.Equipment.Equipment.Barbell
        | Core.Models.Equipment.Equipment.Kettlebells
        | Core.Models.Equipment.Equipment.TRXSystem;
}
