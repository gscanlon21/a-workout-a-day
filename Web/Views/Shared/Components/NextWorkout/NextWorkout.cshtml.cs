using Core.Dtos.Newsletter;
using Core.Models.User;

namespace Web.Views.Shared.Components.NextWorkout;

public class NextWorkoutViewModel
{
    public Days Today { get; init; }

    /// <summary>
    /// Negative if the next workout is in the process of sending.
    /// Otherwise the duration until the next workout starts sending.
    /// </summary>
    public TimeSpan? TimeUntilNextSend { get; init; }

    public bool NextWorkoutSendsToday { get; init; }

    /// <summary>
    /// The rotation type of the next workout.
    /// </summary>
    public Core.Models.Newsletter.WorkoutSplit CurrentAndUpcomingRotations { get; init; } = null!;

    public WorkoutRotationDto MobilityRotation { get; init; } = null!;

    public Data.Entities.User.User User { get; init; } = null!;

    public string Token { get; init; } = null!;
}
