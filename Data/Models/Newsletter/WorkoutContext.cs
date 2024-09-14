using Core.Dtos.Newsletter;
using Core.Models.Exercise;
using Core.Models.User;

namespace Data.Models.Newsletter;

public class WorkoutContext
{
    public required DateOnly Date { get; init; }
    public required string Token { get; init; } = null!;
    public required Data.Entities.User.User User { get; init; } = null!;
    public required MusculoskeletalSystem UserAllWorkedMuscles { get; init; }
    public required WorkoutRotationDto WorkoutRotation { get; init; } = null!;
    public required IDictionary<MusculoskeletalSystem, int?>? WeeklyMusclesRDA { get; init; }
    public required IDictionary<MusculoskeletalSystem, int?>? WeeklyMusclesTUL { get; init; }
    public required double WeeklyMusclesWeeks { get; init; }
    public required Frequency Frequency { get; init; }
    public required Intensity Intensity { get; init; }
    public required TimeSpan TimeUntilDeload { get; init; }
    public required bool NeedsDeload { get; init; }
}
