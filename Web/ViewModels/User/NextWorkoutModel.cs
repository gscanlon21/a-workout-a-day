using Web.Entities.Newsletter;

namespace Web.ViewModels.User;

public class NextWorkoutViewModel
{
    /// <summary>
    /// Negative if the next workout is in the process of sending.
    /// Otherwise the duration until the next workout starts sending.
    /// </summary>
    public TimeSpan? TimeUntilNextSend { get; init; }

    /// <summary>
    /// The rotation type of the next workout.
    /// </summary>
    public NewsletterRotation NextWorkoutType { get; init; } = null!;

    public Entities.User.User User { get; init; } = null!;

    public string Token { get; init; } = null!;
}
