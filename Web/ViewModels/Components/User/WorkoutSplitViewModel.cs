using Data.Models.Newsletter;

namespace Web.ViewModels.Components.User;

public class WorkoutSplitViewModel
{
    /// <summary>
    /// The rotation type of the next workout.
    /// </summary>
    public required WorkoutSplit CurrentAndUpcomingRotations { get; init; } = null!;

    public required Data.Entities.User.User User { get; init; } = null!;
}
