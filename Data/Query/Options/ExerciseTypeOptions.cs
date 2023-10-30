using Core.Models.Exercise;

namespace Data.Query.Options;

public class ExerciseTypeOptions : IOptions
{
    public ExerciseTypeOptions() { }

    public ExerciseTypeOptions(ExerciseType? exerciseType)
    {
        ExerciseType = exerciseType;
    }

    /// <summary>
    /// Any of these exercise types are included.
    /// </summary>
    public ExerciseType? ExerciseType { get; private set; }
}
