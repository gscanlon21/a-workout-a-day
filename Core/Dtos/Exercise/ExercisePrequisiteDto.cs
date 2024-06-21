using Core.Consts;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Core.Dtos.Exercise;

/// <summary>
/// Pre-requisite exercises for other exercises
/// </summary>
[DebuggerDisplay("{Exercise} needs {PrerequisiteExercise}")]
public class ExercisePrerequisiteDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>
    /// The progression level needed to attain proficiency in the exercise
    /// </summary>
    [Required, Range(UserConsts.MinUserProgression, UserConsts.MaxUserProgression)]
    public int Proficiency { get; init; }
}
