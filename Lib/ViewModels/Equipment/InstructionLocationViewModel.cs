using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Lib.ViewModels.Equipment;

/// <summary>
/// Equipment that can be switched out for one another.
/// </summary>
[DebuggerDisplay("Name = {Name}")]
public class InstructionLocationViewModel
{
    [Required]
    public EquipmentLocationViewModel Location { get; init; }

    /// <summary>
    /// A link to show the user how to complete the exercise w/ this equipment.
    /// </summary>
    public string Link { get; init; } = null!;

    public int InstructionId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Equipment.Instruction.Locations))]
    public virtual InstructionViewModel Instruction { get; init; } = null!;
}
