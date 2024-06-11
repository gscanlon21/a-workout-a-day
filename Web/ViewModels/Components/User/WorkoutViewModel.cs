using Core.Models.User;
using Data.Entities.Newsletter;

namespace Web.ViewModels.Components.User;

public class WorkoutViewModel
{
    public required Data.Entities.User.User User { get; init; } = null!;

    public required string Token { get; init; } = null!;

    public required WorkoutRotation? Rotation { get; init; }

    public required Frequency Frequency { get; init; }
}
