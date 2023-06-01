using Web.Models.Exercise;
using Web.Services;

namespace Web.ViewModels.User;

/// <summary>
/// Viewmodel for MonthlyMuscles.cshtml
/// </summary>
public class MonthlyMusclesViewModel
{
    public required Entities.User.User User { get; set; }

    public required IDictionary<MuscleGroups, int> WeeklyTimeUnderTension { get; set; }

    public required double WeeklyTimeUnderTensionAvg { get; set; }

    /// <summary>
    /// The avg minimum number of seconds per week each muscle group should be under tension.
    /// </summary>
    public double MinSecsPerWeek = Math.Round(UserService.MuscleTargets.Values.Average(r => r.Start.Value));

    /// <summary>
    /// The avg maximum number of seconds per week each muscle group should be under tension.
    /// </summary>
    public double MaxSecsPerWeek = Math.Round(UserService.MuscleTargets.Values.Average(r => r.End.Value));

    // The max value (seconds of time-under-tension) of the range display
    public double MaxRangeValue = UserService.MuscleTargets.Values.Max(r => r.End.Value);
}
