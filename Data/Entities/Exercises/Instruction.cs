using Core.Models.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Exercises;

/// <summary>
/// A variation's instructions.
/// </summary>
[Table("instruction")]
[Index(nameof(ParentId))]
[Index(nameof(VariationId))]
[DebuggerDisplay("{Name,nq}")]
public class Instruction
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    public int? ParentId { get; private init; }
    public int VariationId { get; private init; }

    public int? Order { get; private init; }

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

    public Equipment Equipment { get; private set; }

    public string? DisabledReason { get; private init; } = null;


    [NotMapped]
    public Equipment[]? EquipmentBinder
    {
        get => Enum.GetValues<Equipment>().Where(e => Equipment.HasFlag(e)).ToArray();
        set => Equipment = value?.Aggregate(Equipment.None, (a, e) => a | e) ?? Equipment.None;
    }


    #region Navigation Properties

    [InverseProperty(nameof(Parent))]
    public virtual ICollection<Instruction> Children { get; private init; } = [];

    [JsonIgnore, InverseProperty(nameof(Children))]
    public virtual Instruction? Parent { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Exercises.Variation.Instructions))]
    public virtual Variation Variation { get; private init; } = null!;

    #endregion
}
