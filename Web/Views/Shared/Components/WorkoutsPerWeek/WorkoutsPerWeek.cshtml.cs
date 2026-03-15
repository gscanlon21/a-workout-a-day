using Core.Models.User;

namespace Web.Views.Shared.Components.WorkoutsPerWeek;

public class WorkoutsPerWeekViewModel
{
    public Data.Entities.Users.User User { get; }

    public int MaxWorkoutsPerWeek { get; }

    public int MinWorkoutsPerWeek { get; }

    public WorkoutsPerWeekViewModel(Data.Entities.Users.User user)
    {
        User = user;

        (MinWorkoutsPerWeek, MaxWorkoutsPerWeek) = user.Frequency switch
        {
            Frequency.FullBody2Day => (2, 3),
            Frequency.PushPullLeg3Day => (5, 6),
            Frequency.VarietySplit3Day => (3, 4),
            Frequency.UpperLowerBodySplit4Day => (4, 6),
            Frequency.UpperLowerFullBodySplit3Day => (3, 4),
            Frequency.PushPullLegsFullBodySplit4Day => (4, 5),
            Frequency.PushPullLegsUpperLowerSplit5Day => (5, 6),
            Frequency.VarietySplit6Day => (5, 6),
            _ => (2, 6)
        };
    }
}
