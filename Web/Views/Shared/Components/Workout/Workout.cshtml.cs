using Core.Dtos.Newsletter;
using Core.Models.User;

namespace Web.Views.Shared.Components.Workout;

public class WorkoutViewModel
{
    public required Data.Entities.Users.User User { get; init; } = null!;

    public required string Token { get; init; } = null!;

    public required WorkoutRotationDto? Rotation { get; init; }

    public required Frequency Frequency { get; init; }
}
