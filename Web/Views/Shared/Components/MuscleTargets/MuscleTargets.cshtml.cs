using Core.Consts;
using Core.Models.Exercise;
using Data.Entities.User;

namespace Web.Views.Shared.Components.MuscleTargets;


/// <summary>
/// Viewmodel for MonthlyMuscles.cshtml
/// </summary>
public class MuscleTargetsViewModel
{
    public required string Token { get; set; }
    public required Data.Entities.User.User User { get; set; }

    public int Weeks { get; set; }

    public double WeeksOfData { get; set; }

    public required IDictionary<MusculoskeletalSystem, int?> WeeklyVolume { get; set; }

    public MusculoskeletalSystem UsersWorkedMuscles { get; init; }

    // The max value (seconds of time-under-tension) of the range display
    public double MaxRangeValue => UserMuscleStrength.MuscleTargets.Values.Max(r => r.End.Value);

    public MonthlyMuscle GetMuscleTarget(KeyValuePair<MusculoskeletalSystem, Range> defaultRange)
    {
        var userMuscleTarget = User.UserMuscleStrengths.Cast<UserMuscleStrength?>().FirstOrDefault(um => um?.MuscleGroup == defaultRange.Key)?.Range ?? UserMuscleStrength.MuscleTargets[defaultRange.Key];

        return new MonthlyMuscle()
        {
            MuscleGroup = defaultRange.Key,
            UserMuscleTarget = userMuscleTarget,
            Start = userMuscleTarget.Start.Value / MaxRangeValue * 100,
            Middle = (userMuscleTarget.Start.Value + UserConsts.IncrementMuscleTargetBy) / MaxRangeValue * 100,
            End = userMuscleTarget.End.Value / MaxRangeValue * 100,
            DefaultStart = defaultRange.Value.Start.Value / MaxRangeValue * 100,
            DefaultEnd = defaultRange.Value.End.Value / MaxRangeValue * 100,
            ValueInRange = Math.Min(101, (WeeklyVolume[defaultRange.Key] ?? 0) / MaxRangeValue * 100),
            ShowButtons = UsersWorkedMuscles.HasFlag(defaultRange.Key),
        };
    }

    public class MonthlyMuscle
    {
        public double MaxRangeValue => UserMuscleStrength.MuscleTargets.Values.Max(r => r.End.Value);

        public required MusculoskeletalSystem MuscleGroup { get; init; }
        public required Range UserMuscleTarget { get; init; }

        public required double Start { get; init; }
        public double Middle { get; init; }
        public required double End { get; init; }

        public required double DefaultStart { get; init; }
        public required double DefaultEnd { get; init; }

        public required double ValueInRange { get; init; }
        public bool IsMinVolumeInRange => ValueInRange >= Start;
        public bool IsMaxVolumeInRange => ValueInRange <= End;

        public required bool ShowButtons { get; init; }
        public bool ShowDecreaseStart => ShowButtons && UserMuscleTarget.Start.Value > 0;
        public bool ShowIncreaseStart => ShowButtons && UserMuscleTarget.Start.Value + UserConsts.IncrementMuscleTargetBy < UserMuscleTarget.End.Value - UserConsts.IncrementMuscleTargetBy;
        public bool ShowDecreaseEnd => ShowButtons && UserMuscleTarget.End.Value - UserConsts.IncrementMuscleTargetBy > UserMuscleTarget.Start.Value + UserConsts.IncrementMuscleTargetBy;
        public bool ShowIncreaseEnd => ShowButtons && UserMuscleTarget.End.Value < MaxRangeValue;

        public string Color => (IsMinVolumeInRange, IsMaxVolumeInRange, ShowButtons) switch
        {
            // Max volume is out of range
            (_, false, true) => "orangered",
            // Min volume is out of range
            (false, _, true) => "darkorange",
            // Volume is in range
            _ => "limegreen",
        };
    }
}
