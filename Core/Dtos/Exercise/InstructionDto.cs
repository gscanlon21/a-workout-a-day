using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Core.Dtos.Exercise;

/// <summary>
/// Equipment that can be switched out for one another.
/// </summary>
[Table("instruction")]
[DebuggerDisplay("Name = {Name}")]
public class InstructionDto
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int? Order { get; init; }

    /// <summary>
    /// Notes about the variation (not externally shown)
    /// </summary>
    [JsonIgnore]
    public string? Notes { get; init; } = null;

    /// <summary>
    /// Friendly name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// A link to show the user how to complete the exercise w/ this equipment.
    /// </summary>
    public string? Link { get; init; }

    public Models.Equipment.Equipment Equipment { get; init; }

    public string? DisabledReason { get; init; } = null;

    public virtual ICollection<InstructionDto> Children { get; init; } = [];

    [JsonIgnore]
    public virtual InstructionDto? Parent { get; init; } = null!;

    [JsonIgnore]
    public virtual VariationDto Variation { get; init; } = null!;
}
