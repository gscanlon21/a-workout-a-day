using Data.Models.Newsletter;

namespace Web.Views.Shared.Components.PastWorkouts;

public class PastWorkoutsViewModel
{
    public IList<PastWorkout> PastWorkouts { get; init; } = null!;

    public Data.Entities.Users.User User { get; init; } = null!;

    public string Token { get; init; } = null!;
}
