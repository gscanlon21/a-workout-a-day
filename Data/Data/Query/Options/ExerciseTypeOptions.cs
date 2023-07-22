using Core.Models.Exercise;

namespace Data.Data.Query.Options;

public class ExerciseTypeOptions : IOptions
{
    private ExerciseType? _prerequisiteExerciseType;

    public ExerciseTypeOptions() { }

    public ExerciseTypeOptions(ExerciseType? exerciseType)
    {
        ExerciseType = exerciseType;
    }

    /// <summary>
    /// Any of these exercise types are included.
    /// </summary>
    public ExerciseType? ExerciseType { get; private set; }

    /// <summary>
    /// All of these exercise types are checked when checking prerequisites.
    /// </summary>
    public ExerciseType? PrerequisiteExerciseType { get => ((ExerciseType ?? Core.Models.Exercise.ExerciseType.None) | _prerequisiteExerciseType) ?? ExerciseType; set => _prerequisiteExerciseType = value; }
}
