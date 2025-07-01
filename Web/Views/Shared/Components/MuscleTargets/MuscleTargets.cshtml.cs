using Core.Models.Exercise;
using Data.Entities.User;

namespace Web.Views.Shared.Components.MuscleTargets;

/// <summary>
/// Viewmodel for MuscleTargets.cshtml
/// </summary>
public class MuscleTargetsViewModel
{
    public const string Title = "Weekly Muscle Targets";

    public required string Token { get; set; }
    public required Data.Entities.User.User User { get; set; }

    public int Weeks { get; set; }

    public double WeeksOfData { get; set; }

    public MusculoskeletalSystem UsersWorkedMuscles { get; init; }

    public required IDictionary<MusculoskeletalSystem, int?> WeeklyVolume { get; set; }

    /// <summary>
    /// The max value (seconds of time-under-tension) of the range display.
    /// </summary>
    public double MaxRangeValue => UserMuscleStrength.MuscleTargets.Values.Max(r => r.End.Value);

    public MuscleTarget AllMuscleTarget => new()
    {
        MuscleGroup = MusculoskeletalSystem.All,
        ShowButtons = true,
        Start = 0,
        End = 100,
        Middle = 50,
        DefaultStart = 0,
        DefaultEnd = 100,
        ValueInRange = 50,
        UserMuscleTarget = new Range(Convert.ToInt32(MaxRangeValue) / 4, Convert.ToInt32(MaxRangeValue) / 4 * 3),
    };

    public MuscleTarget GetMuscleTarget(KeyValuePair<MusculoskeletalSystem, Range> defaultRange)
    {
        // Show default muscle targets when backfilling data.
        var valueInRange = User.CreatedDate == DateHelpers.Today ? 0 : Math.Min(101, (WeeklyVolume[defaultRange.Key] ?? 0) / MaxRangeValue * 100);
        var userMuscleTarget = User.UserMuscleStrengths.Cast<UserMuscleStrength?>().FirstOrDefault(um => um?.MuscleGroup == defaultRange.Key)?.Range ?? UserMuscleStrength.MuscleTargets[defaultRange.Key];

        return new MuscleTarget()
        {
            ValueInRange = valueInRange,
            MuscleGroup = defaultRange.Key,
            UserMuscleTarget = userMuscleTarget,
            End = userMuscleTarget.End.Value / MaxRangeValue * 100,
            Start = userMuscleTarget.Start.Value / MaxRangeValue * 100,
            Middle = (userMuscleTarget.Start.Value + UserConsts.IncrementMuscleTargetBy) / MaxRangeValue * 100,
            DefaultStart = defaultRange.Value.Start.Value / MaxRangeValue * 100,
            DefaultEnd = defaultRange.Value.End.Value / MaxRangeValue * 100,
            ShowButtons = UsersWorkedMuscles.HasFlag(defaultRange.Key),
        };
    }

    public class MuscleTarget
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
