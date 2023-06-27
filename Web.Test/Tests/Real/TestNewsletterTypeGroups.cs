using Core.Models.Exercise;
using Core.Models.User;
using Data.Models.Newsletter;

namespace Web.Test.Tests.Real;

[TestClass]
public class TestWorkoutSplit : RealDatabase
{
    private static readonly MovementPattern AllMovementPatterns
        = (MovementPattern)(1 << 0)
        | (MovementPattern)(1 << 1)
        | (MovementPattern)(1 << 2)
        | (MovementPattern)(1 << 3)
        | (MovementPattern)(1 << 4)
        | (MovementPattern)(1 << 5)
        | (MovementPattern)(1 << 6)
        | (MovementPattern)(1 << 7)
        | (MovementPattern)(1 << 8);

    #region Movement Patterns

#if DEBUG
    [Ignore]
#endif
    [TestMethod]
    public void WorkoutSplit_MaintainFullBody_HasAllMovementPatterns()
    {
        var groups = new WorkoutSplit(Frequency.FullBody2Day);
        // Works all major movement patterns each week
        Assert.IsTrue(groups.Aggregate((MovementPattern)0, (curr, n) => curr | n.MovementPatterns).HasFlag(AllMovementPatterns));
    }

#if DEBUG
    [Ignore]
#endif
    [TestMethod]
    public void WorkoutSplit_MaintainUpperLower4Day_HasAllMovementPatterns()
    {
        var groups = new WorkoutSplit(Frequency.UpperLowerBodySplit4Day);
        // Works all major movement patterns each week
        Assert.IsTrue(groups.Aggregate((MovementPattern)0, (curr, n) => curr | n.MovementPatterns).HasFlag(AllMovementPatterns));
    }

#if DEBUG
    [Ignore]
#endif
    [TestMethod]
    public void WorkoutSplit_MaintainUpperPushPullLower_HasAllMovementPatterns()
    {
        var groups = new WorkoutSplit(Frequency.PushPullLeg3Day);
        // Works all major movement patterns each week
        Assert.IsTrue(groups.Aggregate((MovementPattern)0, (curr, n) => curr | n.MovementPatterns).HasFlag(AllMovementPatterns));
    }

    #endregion

    #region Muscles

#if DEBUG
    [Ignore]
#endif
    [TestMethod]
    public void WorkoutSplit_MaintainFullBody_HasAllMuscles()
    {
        var groups = new WorkoutSplit(Frequency.FullBody2Day);
        // Works every muscle group except core each week
        Assert.IsTrue(groups.Aggregate(MuscleGroups.None, (curr, n) => curr | n.MuscleGroups).HasFlag(MuscleGroups.UpperLower));
    }

#if DEBUG
    [Ignore]
#endif
    [TestMethod]
    public void WorkoutSplit_MaintainUpperLower4Day_HasAllMuscles()
    {
        var groups = new WorkoutSplit(Frequency.UpperLowerBodySplit4Day);
        // Works every muscle group except core each week
        Assert.IsTrue(groups.Aggregate(MuscleGroups.None, (curr, n) => curr | n.MuscleGroups).HasFlag(MuscleGroups.UpperBody | MuscleGroups.LowerBody));
    }

#if DEBUG
    [Ignore]
#endif
    [TestMethod]
    public void WorkoutSplit_MaintainUpperPushPullLower_HasAllMuscles()
    {
        var groups = new WorkoutSplit(Frequency.PushPullLeg3Day);
        // Works every muscle group except core each week
        Assert.IsTrue(groups.Aggregate(MuscleGroups.None, (curr, n) => curr | n.MuscleGroups).HasFlag(MuscleGroups.UpperBodyPull | MuscleGroups.UpperBodyPush | MuscleGroups.LowerBody));
    }

    #endregion
}