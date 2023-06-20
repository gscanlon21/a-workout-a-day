using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace App.Dtos.Exercise;

/// <summary>
/// Pre-requisite exercises for other exercises
/// </summary>
[Table("exercise_prerequisite")]
[DebuggerDisplay("Name = {Name}")]
public class ExercisePrerequisite
{
    public virtual int ExerciseId { get; private init; }

    [JsonIgnore, InverseProperty(nameof(Dtos.Exercise.Exercise.Prerequisites))]
    public virtual Exercise Exercise { get; private init; } = null!;

    public virtual int PrerequisiteExerciseId { get; private init; }

    [JsonIgnore, InverseProperty(nameof(Dtos.Exercise.Exercise.PrerequisiteExercises))]
    public virtual Exercise PrerequisiteExercise { get; private init; } = null!;
}
