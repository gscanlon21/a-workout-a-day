using Core.Models.Equipment;
using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.Equipment;

/// <summary>
/// Equipment that can be switched out for one another.
/// </summary>
public interface IInstructionLocation
{
    [Required]
    public EquipmentLocation Location { get; init; }

    /// <summary>
    /// A link to show the user how to complete the exercise w/ this equipment.
    /// </summary>
    public string Link { get; init; }

    public int InstructionId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Equipment.Instruction.Locations))]
    public IInstruction Instruction { get; init; }
}
