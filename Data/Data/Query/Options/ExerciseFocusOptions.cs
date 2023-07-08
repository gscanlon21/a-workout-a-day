using Core.Models.Exercise;

namespace Data.Data.Query.Options;

public class ExerciseFocusOptions : IOptions
{
    public ExerciseFocusOptions() { }

    public ExerciseFocusOptions(ExerciseFocus? exerciseFocus)
    {
        ExerciseFocus = exerciseFocus;
    }

    /// <summary>
    /// Will filter down to any of the flags values.
    /// </summary>
    public ExerciseFocus? ExerciseFocus { get; set; }
}
