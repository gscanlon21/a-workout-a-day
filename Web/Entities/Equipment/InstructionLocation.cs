using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Web.Models.Equipment;

namespace Web.Entities.Equipment;

/// <summary>
/// Equipment that can be switched out for one another.
/// </summary>
[Table("instruction_location"), Comment("Instructions that can be switched out for one another")]
[DebuggerDisplay("Name = {Name}")]
public class InstructionLocation
{
    [Required]
    public EquipmentLocation Location { get; private init; }

    /// <summary>
    /// A link to show the user how to complete the exercise w/ this equipment.
    /// </summary>
    public string Link { get; private init; } = null!;

    public int InstructionId { get; private init; }

    [JsonIgnore, InverseProperty(nameof(Entities.Equipment.Instruction.Locations))]
    public virtual Instruction Instruction { get; private init; } = null!;
}
