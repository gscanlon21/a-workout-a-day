using Web.Models.Exercise;

namespace Web.ViewModels.User;

public class MonthlyMusclesModel
{
    public required Entities.User.User User { get; set; }

    public required IDictionary<MuscleGroups, int> WeeklyMusclesWorkedOverMonth { get; set; }

    public required double WeeklyMusclesWorkedOverMonthAvg { get; set; }
}
