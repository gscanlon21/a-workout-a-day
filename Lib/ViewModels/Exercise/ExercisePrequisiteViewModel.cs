using Core.Consts;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Lib.ViewModels.Exercise;

/// <summary>
/// Pre-requisite exercises for other exercises
/// </summary>
[DebuggerDisplay("{Name}: {Proficiency}")]
public class ExercisePrerequisiteViewModel
{
    public int Id { get; init; }

    public string Name { get; init; } = null!;

    /// <summary>
    /// The progression level needed to attain proficiency in the exercise
    /// </summary>
    [Required, Range(UserConsts.MinUserProgression, UserConsts.MaxUserProgression)]
    public int Proficiency { get; init; }

    public override int GetHashCode() => HashCode.Combine(Id);

    public override bool Equals(object? obj) => obj is ExercisePrerequisiteViewModel other
        && other.Id == Id;
}
