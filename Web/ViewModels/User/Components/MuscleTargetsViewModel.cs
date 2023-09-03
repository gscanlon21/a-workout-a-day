using Core.Models.Exercise;
using Data.Entities.User;

namespace Web.ViewModels.User.Components;

/// <summary>
/// Viewmodel for MonthlyMuscles.cshtml
/// </summary>
public class MuscleTargetsViewModel
{
    public required string Token { get; set; }
    public required Data.Entities.User.User User { get; set; }

    public int Weeks { get; set; }

    public required IDictionary<MuscleGroups, int?> WeeklyVolume { get; set; }

    public MuscleGroups UsersWorkedMuscles { get; init; }

    // The max value (seconds of time-under-tension) of the range display
    public double MaxRangeValue => UserMuscleStrength.MuscleTargets.Values.Max(r => r.End.Value);

    public MonthlyMuscle GetMuscleTarget(KeyValuePair<MuscleGroups, Range> defaultRange)
    {
        var userMuscleTarget = User.UserMuscleStrengths.Cast<UserMuscleStrength?>().FirstOrDefault(um => um?.MuscleGroup == defaultRange.Key)?.Range ?? UserMuscleStrength.MuscleTargets[defaultRange.Key];

        return new MonthlyMuscle()
        {
            MuscleGroup = defaultRange.Key,
            UserMuscleTarget = userMuscleTarget,
            Start = userMuscleTarget.Start.Value / MaxRangeValue * 100,
            End = userMuscleTarget.End.Value / MaxRangeValue * 100,
            DefaultStart = defaultRange.Value.Start.Value / MaxRangeValue * 100,
            DefaultEnd = defaultRange.Value.End.Value / MaxRangeValue * 100,
            ValueInRange = Math.Min(101, (WeeklyVolume[defaultRange.Key] ?? 0) / MaxRangeValue * 100),
            IsMinVolumeInRange = WeeklyVolume[defaultRange.Key] >= userMuscleTarget.Start.Value,
            IsMaxVolumeInRange = WeeklyVolume[defaultRange.Key] <= userMuscleTarget.End.Value,
            ShowButtons = UsersWorkedMuscles.HasFlag(defaultRange.Key),
        };
    }

    public class MonthlyMuscle
    {
        public required MuscleGroups MuscleGroup { get; init; }
        public required bool ShowButtons { get; init; }
        public required Range UserMuscleTarget { get; init; }
        public required double Start { get; init; }
        public double Middle => (End + Start) / 2d;
        public required double End { get; init; }
        public required double DefaultStart { get; init; }
        public required double DefaultEnd { get; init; }
        public required double ValueInRange { get; init; }
        public required bool IsMinVolumeInRange { get; init; }
        public required bool IsMaxVolumeInRange { get; init; }
    }
}
