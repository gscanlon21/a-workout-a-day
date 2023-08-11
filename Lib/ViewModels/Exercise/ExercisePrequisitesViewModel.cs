using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Lib.ViewModels.Exercise;

/// <summary>
/// Pre-requisite exercises for other exercises
/// </summary>
[DebuggerDisplay("Name = {Name}")]
public class ExercisePrerequisite
{
    public int ExerciseId { get; init; }

    [JsonInclude]
    public ExerciseViewModel Exercise { get; init; } = null!;

    public int PrerequisiteExerciseId { get; init; }

    [JsonInclude]
    public ExerciseViewModel PrerequisiteExercise { get; init; } = null!;
}
