using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;
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
    [Required]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Similar groups of exercises.
    /// </summary>
    [Required]
    public ExerciseGroup Groups { get; init; }

    /// <summary>
    /// Multiplier for how often this exercise is choosen. Weights the LastSeen date.
    /// </summary>
    [Required]
    public bool IsCore { get; set; }

    /// <summary>
    /// Notes about the variation (externally shown).
    /// </summary>
    public string? Notes { get; init; } = null;

    public string? DisabledReason { get; init; } = null;

    public override int GetHashCode() => HashCode.Combine(Id);

    public override bool Equals(object? obj) => obj is ExerciseViewModel other
        && other.Id == Id;
}
