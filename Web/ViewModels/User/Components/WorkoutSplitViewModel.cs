using Data.Models.Newsletter;

namespace Web.ViewModels.User.Components;

public class WorkoutSplitViewModel
{
    /// <summary>
    /// The rotation type of the next workout.
    /// </summary>
    public WorkoutSplit CurrentAndUpcomingRotations { get; init; } = null!;
}
