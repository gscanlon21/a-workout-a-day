using Data.Models.Newsletter;

namespace Web.ViewModels.User;

public class WorkoutSplitViewModel
{
    /// <summary>
    /// The rotation type of the next workout.
    /// </summary>
    public NewsletterTypeGroups CurrentAndUpcomingRotations { get; init; } = null!;
}
