using Core.Models.Exercise.Skills;
using System.Diagnostics;

namespace Core.Dtos.Exercise;

/// <summary>
/// Exercises listed on the website
/// </summary>
[DebuggerDisplay("{Name,nq}")]
public class ExerciseDto
{
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Similar groups of exercises.
    /// </summary>
    public SkillTypes SkillType { get; init; }

    public int Skills { get; init; }

    public Enum? UnusedSkills => SkillType switch
    {
        SkillTypes.VisualSkills => VisualSkills.All & ~(VisualSkills)Skills,
        SkillTypes.CervicalSkills => CervicalSkills.All & ~(CervicalSkills)Skills,
        SkillTypes.ThoracicSkills => ThoracicSkills.All & ~(ThoracicSkills)Skills,
        SkillTypes.LumbarSkills => LumbarSkills.All & ~(LumbarSkills)Skills,
        _ => null,
    };

    public Enum? TypedSkills => SkillType switch
    {
        SkillTypes.VisualSkills => (VisualSkills)Skills,
        SkillTypes.CervicalSkills => (CervicalSkills)Skills,
        SkillTypes.ThoracicSkills => (ThoracicSkills)Skills,
        SkillTypes.LumbarSkills => (LumbarSkills)Skills,
        _ => null,
    };

    /// <summary>
    /// Notes about the variation (externally shown).
    /// </summary>
    public string? Notes { get; init; } = null;

    public override int GetHashCode() => HashCode.Combine(Id);
    public override bool Equals(object? obj) => obj is ExerciseDto other
        && other.Id == Id;
}
