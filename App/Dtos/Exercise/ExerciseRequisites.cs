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
    public virtual int ExerciseId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Dtos.Exercise.Exercise.Prerequisites))]
    public virtual Exercise Exercise { get; init; } = null!;

    public virtual int PrerequisiteExerciseId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Dtos.Exercise.Exercise.PrerequisiteExercises))]
    public virtual Exercise PrerequisiteExercise { get; init; } = null!;
}
