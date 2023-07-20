using Core.Models.User;
using Data.Entities.Newsletter;

namespace Web.ViewModels.User.Components;

public class WorkoutViewModel
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public Data.Entities.User.User User { get; init; } = null!;

    public string Token { get; init; } = null!;

    public WorkoutRotation? NextRotation { get; init; }

    public Frequency NextFrequency { get; init; }
}
