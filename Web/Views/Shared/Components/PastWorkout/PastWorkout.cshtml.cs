using Data.Entities.Newsletter;

namespace Web.Views.Shared.Components.PastWorkout;

public class PastWorkoutViewModel
{
    public IList<UserWorkout> PastWorkouts { get; init; } = null!;

    public Data.Entities.User.User User { get; init; } = null!;

    public string Token { get; init; } = null!;
}
