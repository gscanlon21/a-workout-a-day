using Lib.ViewModels.Exercise;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Lib.ViewModels.Equipment;

/// <summary>
/// Equipment that can be switched out for one another.
/// </summary>
[DebuggerDisplay("Name = {Name}")]
public class InstructionViewModel
{
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; init; } = null!;

    /// <summary>
    /// A link to show the user how to complete the exercise w/ this equipment.
    /// </summary>
    public string? Link { get; init; }

    public string? DisabledReason { get; init; } = null;

    //[JsonIgnore, InverseProperty(nameof(InstructionLocation.Instruction))]
    public virtual IList<InstructionLocationViewModel> Locations { get; init; } = new List<InstructionLocationViewModel>();

    //[JsonIgnore, InverseProperty(nameof(Parent))]
    public virtual ICollection<InstructionViewModel> Children { get; init; } = new List<InstructionViewModel>();

    //[JsonIgnore, InverseProperty(nameof(Children))]
    public virtual InstructionViewModel? Parent { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(EquipmentDto.Instructions))]
    public virtual ICollection<EquipmentViewModel> Equipment { get; init; } = new List<EquipmentViewModel>();

    //[JsonIgnore, InverseProperty(nameof(Exercise.Variation.Instructions))]
    public virtual VariationViewModel Variation { get; init; } = null!;
}
