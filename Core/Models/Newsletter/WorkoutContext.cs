using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Exercise;
using Core.Models.User;

namespace Core.Models.Newsletter;

public class WorkoutContext
{
    public required UserDto User { get; init; } = null!;
    public required string Token { get; init; } = null!;
    public required WorkoutRotationDto WorkoutRotation { get; init; } = null!;
    public required MuscleGroups UserAllWorkedMuscles { get; init; }
    public required bool NeedsDeload { get; init; }
    public required TimeSpan TimeUntilDeload { get; init; }
    public required IDictionary<MuscleGroups, int?>? WeeklyMusclesRDA { get; init; }
    public required IDictionary<MuscleGroups, int?>? WeeklyMusclesTUL { get; init; }
    public required double WeeklyMusclesWeeks { get; init; }
    public required Frequency Frequency { get; init; }
    public required Intensity Intensity { get; init; }
}
