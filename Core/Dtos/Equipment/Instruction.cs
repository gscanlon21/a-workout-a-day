using Core.Dtos.Exercise;
using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.Equipment;

/// <summary>
/// Equipment that can be switched out for one another.
/// </summary>
public interface IInstruction
{
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; init; }

    /// <summary>
    /// A link to show the user how to complete the exercise w/ this equipment.
    /// </summary>
    public string? Link { get; init; }

    public string? DisabledReason { get; init; }

    //[JsonIgnore, InverseProperty(nameof(InstructionLocation.Instruction))]
    public IList<IInstructionLocation> Locations { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Parent))]
    public ICollection<IInstruction> Children { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Children))]
    public IInstruction? Parent { get; init; }

    //[JsonIgnore, InverseProperty(nameof(EquipmentDto.Instructions))]
    public ICollection<IEquipment> Equipment { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Exercise.Variation.Instructions))]
    public IVariation Variation { get; init; }
}
