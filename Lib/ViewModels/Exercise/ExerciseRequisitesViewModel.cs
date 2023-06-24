using System.Diagnostics;

namespace Lib.ViewModels.Exercise;

/// <summary>
/// Pre-requisite exercises for other exercises
/// </summary>
[DebuggerDisplay("Name = {Name}")]
public class ExercisePrerequisite
{
    public virtual int ExerciseId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.Exercise.Exercise.Prerequisites))]
    public virtual ExerciseViewModel Exercise { get; init; } = null!;

    public virtual int PrerequisiteExerciseId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.Exercise.Exercise.PrerequisiteExercises))]
    public virtual ExerciseViewModel PrerequisiteExercise { get; init; } = null!;
}
