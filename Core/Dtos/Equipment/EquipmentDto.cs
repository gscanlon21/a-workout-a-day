using Core.Dtos.User;
using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.Equipment;

/// <summary>
/// Equipment used in an exercise.
/// </summary>
public interface IEquipment
{
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; init; }

    public string? DisabledReason { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Instruction.Equipment))]
    public ICollection<IInstruction> Instructions { get; init; }

    //[JsonIgnore, InverseProperty(nameof(UserEquipment.Equipment))]
    public ICollection<IUserEquipment> UserEquipments { get; init; }
}
