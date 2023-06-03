using Web.Entities.Newsletter;
using Web.Models.Newsletter;
using Web.Models.User;

namespace Web.ViewModels.User;

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
    public NewsletterTypeGroups CurrentAndUpcomingRotations { get; init; } = null!;

    public Entities.User.User User { get; init; } = null!;

    public string Token { get; init; } = null!;
}
