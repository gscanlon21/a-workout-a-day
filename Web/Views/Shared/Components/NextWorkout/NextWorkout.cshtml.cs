using Core.Models.User;
using Data.Entities.Newsletter;

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
    public Data.Models.Newsletter.WorkoutSplit CurrentAndUpcomingRotations { get; init; } = null!;

    public WorkoutRotation MobilityRotation { get; init; } = null!;

    public Data.Entities.User.User User { get; init; } = null!;

    public string Token { get; init; } = null!;
}
