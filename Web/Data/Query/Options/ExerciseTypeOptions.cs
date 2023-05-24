using Web.Models.Exercise;

namespace Web.Data.Query.Options;

public class ExerciseTypeOptions
{
    private ExerciseType? _prerequisiteExerciseType;

    public ExerciseTypeOptions() { }

    public ExerciseTypeOptions(ExerciseType? exerciseType)
    {
        ExerciseType = exerciseType;
    }

    /// <summary>
    /// 
    /// </summary>
    public ExerciseType? ExerciseType { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public ExerciseType? PrerequisiteExerciseType { get => ((ExerciseType ?? Models.Exercise.ExerciseType.None) | _prerequisiteExerciseType) ?? ExerciseType; set => _prerequisiteExerciseType = value; }
}
