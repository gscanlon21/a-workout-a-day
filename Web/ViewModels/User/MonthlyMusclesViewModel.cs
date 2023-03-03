using Web.Models.Exercise;

namespace Web.ViewModels.User;

/// <summary>
/// Viewmodel for MonthlyMuscles.cshtml
/// </summary>
public class MonthlyMusclesViewModel
{
    public required Entities.User.User User { get; set; }

    public required IDictionary<MuscleGroups, int> WeeklyTimeUnderTension { get; set; }

    public required double WeeklyTimeUnderTensionAvg { get; set; }
}
