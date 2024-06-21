namespace Web.Views.Shared.Components.WorkoutsPerWeek;

public class WorkoutsPerWeekViewModel
{
    public Data.Entities.User.User User { get; }

    public int MaxWorkoutsPerWeek { get; }

    public int MinWorkoutsPerWeek { get; }

    public WorkoutsPerWeekViewModel(Data.Entities.User.User user)
    {
        User = user;

        (MinWorkoutsPerWeek, MaxWorkoutsPerWeek) = user.Frequency switch
        {
            Core.Models.User.Frequency.FullBody2Day => (2, 3),
            Core.Models.User.Frequency.PushPullLeg3Day => (6, 6),
            Core.Models.User.Frequency.UpperLowerBodySplit4Day => (4, 6),
            Core.Models.User.Frequency.UpperLowerFullBodySplit3Day => (3, 4),
            Core.Models.User.Frequency.PushPullLegsFullBodySplit4Day => (4, 5),
            Core.Models.User.Frequency.PushPullLegsUpperLowerSplit5Day => (5, 6),
            _ => (2, 6)
        };
    }
}
