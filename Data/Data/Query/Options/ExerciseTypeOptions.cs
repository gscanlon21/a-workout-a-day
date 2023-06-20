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
    /// 
    /// </summary>
    public ExerciseType? ExerciseType { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public ExerciseType? PrerequisiteExerciseType { get => ((ExerciseType ?? Core.Models.Exercise.ExerciseType.None) | _prerequisiteExerciseType) ?? ExerciseType; set => _prerequisiteExerciseType = value; }
}
