using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Exercises;

/// <summary>
/// Pre-requisite exercises for other exercises.
/// Variation-Exercise prerequisites?
/// </summary>
[Table("exercise_alternative")]
[DebuggerDisplay("{Exercise} needs {AlternativeExercise}")]
public class ExerciseAlternative
{
    /// <summary>
    /// The Id of the postrequisite exercise.
    /// </summary>
    public virtual int ExerciseId { get; private init; }

    public virtual int AlternativeExerciseId { get; private init; }

    /// <summary>
    /// The postrequisite exercise.
    /// </summary>
    [JsonIgnore, InverseProperty(nameof(Exercises.Exercise.Alternatives))]
    public virtual Exercise Exercise { get; private init; } = null!;

    //[InverseProperty(nameof(Exercises.Exercise.Postrequisites))]
    public virtual Exercise AlternativeExercise { get; private init; } = null!;

    public bool? Strict { get; init; }

    //public AlternativeExerciseType OppositeVsAlt { get; private init; }
}
