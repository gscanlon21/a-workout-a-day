using App.Dtos.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace App.Dtos.Equipment;

/// <summary>
/// Equipment that can be switched out for one another.
/// </summary>
[Table("instruction")]
[DebuggerDisplay("Name = {Name}")]
public class Instruction
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

    [JsonIgnore, InverseProperty(nameof(InstructionLocation.Instruction))]
    public virtual IList<InstructionLocation> Locations { get; init; } = new List<InstructionLocation>();

    [JsonIgnore, InverseProperty(nameof(Parent))]
    public virtual ICollection<Instruction> Children { get; init; } = new List<Instruction>();

    [JsonIgnore, InverseProperty(nameof(Children))]
    public virtual Instruction? Parent { get; init; } = null!;

    [JsonIgnore, InverseProperty(nameof(EquipmentDto.Instructions))]
    public virtual ICollection<EquipmentDto> Equipment { get; init; } = new List<EquipmentDto>();

    [JsonIgnore, InverseProperty(nameof(Exercise.Variation.Instructions))]
    public virtual Variation Variation { get; init; } = null!;
}
