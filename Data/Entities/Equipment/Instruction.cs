using Data.Entities.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Equipment;

/// <summary>
/// Equipment that can be switched out for one another.
/// </summary>
[Table("instruction"), Comment("Equipment that can be switched out for one another")]
[DebuggerDisplay("Name = {Name}")]
public class Instruction
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    /// <summary>
    /// Notes about the variation (not externally shown)
    /// </summary>
    [JsonIgnore]
    public string? Notes { get; private init; } = null;

    /// <summary>
    /// Friendly name.
    /// </summary>
    public string? Name { get; private init; }

    /// <summary>
    /// A link to show the user how to complete the exercise w/ this equipment.
    /// </summary>
    public string? Link { get; private init; }

    public Core.Models.Equipment.Equipment Equipment { get; private init; }

    public string? DisabledReason { get; private init; } = null;

    [InverseProperty(nameof(Parent))]
    public virtual ICollection<Instruction> Children { get; private init; } = [];

    [JsonIgnore, InverseProperty(nameof(Children))]
    public virtual Instruction? Parent { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Exercise.Variation.Instructions))]
    public virtual Variation Variation { get; private init; } = null!;
}
