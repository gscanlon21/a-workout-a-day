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

    public const Core.Models.Exercise.Equipment ResistanceEquipments =
        Core.Models.Exercise.Equipment.ResistanceBands
        | Core.Models.Exercise.Equipment.GymnasticRings
        | Core.Models.Exercise.Equipment.Dumbbells
        | Core.Models.Exercise.Equipment.Barbell
        | Core.Models.Exercise.Equipment.Kettlebells
        | Core.Models.Exercise.Equipment.TRXSystem;
}
