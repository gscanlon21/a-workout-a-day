using Core.Consts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Exercise;

/// <summary>
/// Pre-requisite exercises for other exercises
/// </summary>
[Table("exercise_prerequisite")]
[DebuggerDisplay("{Exercise} needs {PrerequisiteExercise}")]
public class ExercisePrerequisite
{
    public virtual int PrerequisiteExerciseId { get; private init; }

    [InverseProperty(nameof(Entities.Exercise.Exercise.Postrequisites))]
    public virtual Exercise PrerequisiteExercise { get; private init; } = null!;

    /// <summary>
    /// The Id of the postrequisite exercise.
    /// </summary>
    public virtual int ExerciseId { get; private init; }

    /// <summary>
    /// The postrequisite exercise.
    /// </summary>
    [JsonIgnore, InverseProperty(nameof(Entities.Exercise.Exercise.Prerequisites))]
    public virtual Exercise Exercise { get; private init; } = null!;

    /// <summary>
    /// The progression level of the prerequisite the user needs to be at to unlock the postrequisite.
    /// </summary>
    [Required, Range(UserConsts.MinUserProgression, UserConsts.MaxUserProgression)]
    public int Proficiency { get; private init; }
}
