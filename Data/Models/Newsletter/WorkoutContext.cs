using Core.Models.Exercise;
using Core.Models.User;
using Data.Entities.Newsletter;
using Data.Entities.User;

namespace Data.Models.Newsletter;

internal class WorkoutContext
{
    public required User User { get; init; } = null!;
    public required WorkoutRotation WorkoutRotation { get; init; } = null!;
    public required MuscleGroups UserAllWorkedMuscles { get; init; }
    public required bool NeedsDeload { get; init; }
    public required TimeSpan TimeUntilDeload { get; init; }
    public required IDictionary<MuscleGroups, int?>? WeeklyMuscles { get; init; }
    public required Frequency Frequency { get; init; }
}
