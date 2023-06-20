using App.Models.Equipment;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace App.Dtos.Equipment;

/// <summary>
/// Equipment that can be switched out for one another.
/// </summary>
[Table("instruction_location")]
[DebuggerDisplay("Name = {Name}")]
public class InstructionLocation
{
    [Required]
    public EquipmentLocation Location { get; init; }

    /// <summary>
    /// A link to show the user how to complete the exercise w/ this equipment.
    /// </summary>
    public string Link { get; init; } = null!;

    public int InstructionId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Equipment.Instruction.Locations))]
    public virtual Instruction Instruction { get; init; } = null!;
}
