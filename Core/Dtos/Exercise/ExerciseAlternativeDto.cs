using System.Diagnostics;

namespace Core.Dtos.Exercise;

/// <summary>
/// Alternative exercises for other exercises.
/// </summary>
[DebuggerDisplay("Id = {Id}, Name = {Name,nq}")]
public class ExerciseAlternativeDto
{
    /// <summary>
    /// The Id of the alternative exercise.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The Name of the alternative exercise.
    /// </summary>
    public string Name { get; init; } = null!;

    public bool? Strict { get; init; }

    public string HtmlString => Strict switch
    {
        false => $"<i>{Name}</i>",
        true => $"<b>{Name}</b>",
        null => $"{Name}",
    };
}
