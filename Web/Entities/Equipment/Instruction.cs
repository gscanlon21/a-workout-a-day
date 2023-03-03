using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Web.Entities.Exercise;

namespace Web.Entities.Equipment;

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
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; private init; } = null!;

    /// <summary>
    /// Whether the equipment in the equipment group is used as weight/resistence for a harder workout.
    /// </summary>
    [Required]
    public bool IsWeight { get; private init; }

    /// <summary>
    /// A link to show the user how to complete the exercise w/ this equipment.
    /// </summary>
    public string? Link { get; private init; }

    public string? DisabledReason { get; private init; } = null;

    [InverseProperty(nameof(InstructionLocation.Instruction))]
    public virtual IList<InstructionLocation> Locations { get; private init; } = null!;

    [InverseProperty(nameof(Parent))]
    public virtual ICollection<Instruction> Children { get; private init; } = new List<Instruction>();

    [InverseProperty(nameof(Children))]
    public virtual Instruction? Parent { get; private init; } = null!;

    [InverseProperty(nameof(Entities.Equipment.Equipment.Instructions))]
    public virtual ICollection<Equipment> Equipment { get; private init; } = null!;

    [InverseProperty(nameof(Exercise.Variation.Instructions))]
    public virtual Variation Variation { get; private init; } = null!;
}
