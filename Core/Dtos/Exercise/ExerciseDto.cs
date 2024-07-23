using Core.Dtos.User;
using Core.Models.Exercise.Skills;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Core.Dtos.Exercise;

/// <summary>
/// Exercises listed on the website
/// </summary>
[Table("exercise")]
[DebuggerDisplay("{Name,nq}")]
public class ExerciseDto
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Similar groups of exercises.
    /// </summary>
    [Required]
    public SkillTypes SkillType { get; init; }

    [Required]
    public int Skills { get; init; }

    public Enum? UnusedSkills => SkillType switch
    {
        SkillTypes.VisualSkills => VisualSkills.All & ~(VisualSkills)Skills,
        _ => null,
    };

    [NotMapped]
    public Enum? TypedSkills => SkillType switch
    {
        SkillTypes.VisualSkills => (VisualSkills)Skills,
        _ => null,
    };

    /// <summary>
    /// Notes about the variation (externally shown).
    /// </summary>
    public string? Notes { get; init; } = null;

    public string? DisabledReason { get; init; } = null;

    public virtual ICollection<ExercisePrerequisiteDto> Prerequisites { get; init; } = null!;

    [JsonIgnore]
    public virtual ICollection<ExercisePrerequisiteDto> Postrequisites { get; init; } = null!;

    [JsonIgnore]
    public virtual ICollection<VariationDto> Variations { get; init; } = null!;

    [JsonIgnore]
    public virtual ICollection<UserExerciseDto> UserExercises { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id);

    public override bool Equals(object? obj) => obj is ExerciseDto other
        && other.Id == Id;
}
