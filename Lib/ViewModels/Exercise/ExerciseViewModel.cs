using Core.Models.Exercise.Skills;
using System.Diagnostics;

namespace Lib.ViewModels.Exercise;

/// <summary>
/// Exercises listed on the website
/// </summary>
[DebuggerDisplay("{Name,nq}")]
public class ExerciseViewModel
{
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// The type of skills.
    /// </summary>
    public SkillTypes SkillType { get; init; }

    /// <summary>
    /// Similar groups of exercises.
    /// </summary>
    public int Skills { get; init; }

    public Enum TypedSkills => SkillType switch
    {
        SkillTypes.VisualSkills => (VisualSkills)Skills,
        _ => (WorkoutSkills)Skills,
    };

    /// <summary>
    /// Notes about the variation (externally shown).
    /// </summary>
    public string? Notes { get; init; } = null;

    public string? DisabledReason { get; init; } = null;

    public override int GetHashCode() => HashCode.Combine(Id);

    public override bool Equals(object? obj) => obj is ExerciseViewModel other
        && other.Id == Id;
}
