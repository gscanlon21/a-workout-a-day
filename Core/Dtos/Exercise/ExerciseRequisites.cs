namespace Core.Dtos.Exercise;

/// <summary>
/// Pre-requisite exercises for other exercises
/// </summary>
public interface IExercisePrerequisite
{
    public int ExerciseId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.Exercise.Exercise.Prerequisites))]
    public IExercise Exercise { get; init; }

    public int PrerequisiteExerciseId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.Exercise.Exercise.PrerequisiteExercises))]
    public IExercise PrerequisiteExercise { get; init; }
}
