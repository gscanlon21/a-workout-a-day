using Core.Models.User;
using Data.Entities.Newsletter;

namespace Data.Models.Newsletter;

/// <summary>
/// Past workout info.
/// </summary>
public class PastWorkout
{
    [Obsolete("Public parameterless constructor required for model binding.", error: true)]
    public PastWorkout() { }

    public PastWorkout(UserWorkout userWorkout)
    {
        Id = userWorkout.Id;
        Date = userWorkout.Date;
        Rotation = userWorkout.Rotation;
        Frequency = userWorkout.Frequency;
    }

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
    public WorkoutRotation Rotation { get; set; } = null!;

    /// <summary>
    /// What was the workout split used when this newsletter was sent?
    /// </summary>
    public Frequency Frequency { get; init; }
}
