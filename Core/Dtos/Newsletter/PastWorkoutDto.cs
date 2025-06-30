using Core.Models.User;

namespace Core.Dtos.Newsletter;

/// <summary>
/// DTO for PastWorkout.cs
/// </summary>
public class PastWorkoutDto
{
    /// <summary>
    /// The id of the newsletter.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The date the newsletter was sent out on.
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// What day of the workout split was used?
    /// </summary>
    public WorkoutRotationDto Rotation { get; init; } = null!;

    /// <summary>
    /// What was the workout split used when this newsletter was sent?
    /// </summary>
    public Frequency Frequency { get; init; }

    public string Title()
    {
        return Date.ToLongDateString();
    }

    public string Description()
    {
        var first = Frequency == Frequency.Mobility ? Frequency.GetSingleDisplayName() : Rotation.MuscleGroupsDisplayName;
        var second = Frequency == Frequency.Mobility ? Rotation.MuscleGroupsDisplayName : Rotation.MovementPatterns.GetDisplayName(DisplayType.ShortName);
        return $"{first} - {second}";
    }
}
