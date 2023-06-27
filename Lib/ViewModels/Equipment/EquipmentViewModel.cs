using Lib.ViewModels.User;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Lib.ViewModels.Equipment;

/// <summary>
/// Equipment used in an exercise.
/// </summary>
[DebuggerDisplay("Name = {Name}")]
public class EquipmentViewModel
{
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; init; } = null!;

    public string? DisabledReason { get; init; } = null;

    //[JsonIgnore, InverseProperty(nameof(Instruction.Equipment))]
    public virtual ICollection<InstructionViewModel> Instructions { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(UserEquipment.Equipment))]
    public virtual ICollection<UserEquipmentViewModel> UserEquipments { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id);

    public override bool Equals(object? obj) => obj is EquipmentViewModel other
        && other.Id == Id;
}
