using Core.Models.Exercise;
using Core.Models.User;

namespace Core.Dtos.Workout;

/// <summary>
/// Past workout info.
/// </summary>
public class PastWorkoutViewModel
{
    public int Id { get; init; }

    public string Title()
    {
        return Date.ToLongDateString();
    }

    public string Description()
    {
        var first = Frequency == Frequency.OffDayStretches ? Frequency.GetSingleDisplayName() : Rotation.MuscleGroupsDisplayName;
        var second = Frequency == Frequency.OffDayStretches ? Rotation.MuscleGroupsDisplayName : Rotation.MovementPatterns.GetDisplayName32(DisplayType.ShortName);
        return $"{first} - {second}";
    }

    public int UserId { get; init; }

    /// <summary>
    /// The date the newsletter was sent out on
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// What day of the workout split was used?
    /// </summary>
    public WorkoutRotationViewModel Rotation { get; set; } = null!;

    /// <summary>
    /// What was the workout split used when this newsletter was sent?
    /// </summary>
    public Frequency Frequency { get; init; }

    /// <summary>
    /// What was the workout split used when this newsletter was sent?
    /// </summary>
    public Intensity Intensity { get; init; }

    /// <summary>
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate
    /// </summary>
    public bool IsDeloadWeek { get; init; }
}
