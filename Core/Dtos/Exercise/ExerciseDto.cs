using Core.Models.Exercise.Skills;
using System.Diagnostics;

namespace Core.Dtos.Exercise;

/// <summary>
/// DTO class for Exercise.cs
/// </summary>
[DebuggerDisplay("{Name,nq}")]
public class ExerciseDto
{
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    public string Name { get; init; } = null!;

    public VisualSkills VisualSkills { get; init; }
    public CervicalSkills CervicalSkills { get; init; }
    public ThoracicSkills ThoracicSkills { get; init; }
    public LumbarSkills LumbarSkills { get; init; }

    /// <summary>
    /// Notes about the variation (externally shown).
    /// </summary>
    public string? Notes { get; init; } = null;

    public override int GetHashCode() => HashCode.Combine(Id);
    public override bool Equals(object? obj) => obj is ExerciseDto other
        && other.Id == Id;
}
