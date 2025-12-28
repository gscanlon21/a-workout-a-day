namespace Web.Views.Shared.Components.WorkoutSplit;

public class WorkoutSplitViewModel
{
    /// <summary>
    /// The rotation type of the next workout.
    /// </summary>
    public required Data.Models.Newsletter.WorkoutSplit CurrentAndUpcomingRotations { get; init; } = null!;

    public required Data.Entities.Users.User User { get; init; } = null!;
}
