using Core.Models.Exercise;

namespace Data.Query.Options;

public class ExerciseFocusOptions : IOptions
{
    public ExerciseFocusOptions() { }

    public ExerciseFocusOptions(IList<ExerciseFocus>? exerciseFocus)
    {
        ExerciseFocus = exerciseFocus;
    }

    /// <summary>
    /// Will filter down to any of the flags values.
    /// </summary>
    public IList<ExerciseFocus>? ExerciseFocus { get; set; }

    /// <summary>
    /// Exclude this exercise focus. Excludes via HasFlag.
    /// </summary>
    public IList<ExerciseFocus>? ExcludeExerciseFocus { get; set; }

    public bool HasData() => true;
}
