using Core.Dtos.Newsletter;
using Core.Models.Exercise;
using Core.Models.User;

namespace Data.Models.Newsletter;

public class WorkoutContext
{
    public required DateOnly Date { get; init; }
    public required string Token { get; init; } = null!;
    public required Entities.Users.User User { get; init; } = null!;
    public required MusculoskeletalSystem UserAllWorkedMuscles { get; init; }
    public required WorkoutRotationDto WorkoutRotation { get; init; } = null!;
    public required IDictionary<MusculoskeletalSystem, int?>? WeeklyMusclesRDA { get; init; }
    public required IDictionary<MusculoskeletalSystem, int?>? WeeklyMusclesTUL { get; init; }
    public required double WeeklyMusclesWeeks { get; init; }
    public required TimeSpan TimeUntilDeload { get; init; }
    public required Intensity Intensity { get; init; }
    public required bool NeedsDeload { get; init; }

    /// <summary>
    /// The Frequency of the workout. 
    /// Not the user's Frequency.
    /// </summary>
    public required Frequency Frequency { get; init; }

    /// <summary>
    /// Is this workout being generated for a date in the past.
    /// </summary>
    public bool IsBackfill => Date != User.TodayOffset;
}
