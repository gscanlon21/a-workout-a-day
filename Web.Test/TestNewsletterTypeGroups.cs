using Web.Models.Newsletter;
using Web.Models.User;
using Web.Models.Exercise;
using Web.Extensions;

namespace Web.Test;

[TestClass]
public class TestNewsletterTypeGroups
{
    private static readonly MovementPattern AllMovementPatterns 
        = MovementPattern.HorizontalPush 
        | MovementPattern.HorizontalPull
        | MovementPattern.VerticalPush
        | MovementPattern.VerticalPull
        | MovementPattern.HipHinge
        | MovementPattern.Squat
        | MovementPattern.Lunge
        | MovementPattern.Carry
        | MovementPattern.Rotation;

    [TestMethod]
    public void NewsletterTypeGroups_MaintainFullBody_HasAllMovementPatterns()
    {
        var groups = new NewsletterTypeGroups(StrengtheningPreference.Maintain, Frequency.FullBody2Day);
        Assert.IsTrue(groups.Aggregate((MovementPattern)0, (curr, n) => curr | n.MovementPatterns).HasFlag(AllMovementPatterns));
    }

    [TestMethod]
    public void NewsletterTypeGroups_MaintainUpperLower2Day_HasAllMovementPatterns()
    {
        var groups = new NewsletterTypeGroups(StrengtheningPreference.Maintain, Frequency.UpperLowerBodySplit2Day);
        Assert.IsTrue(groups.Aggregate((MovementPattern)0, (curr, n) => curr | n.MovementPatterns).HasFlag(AllMovementPatterns));
    }

    [TestMethod]
    public void NewsletterTypeGroups_MaintainUpperLower4Day_HasAllMovementPatterns()
    {
        var groups = new NewsletterTypeGroups(StrengtheningPreference.Maintain, Frequency.UpperLowerBodySplit4Day);
        Assert.IsTrue(groups.Aggregate((MovementPattern)0, (curr, n) => curr | n.MovementPatterns).HasFlag(AllMovementPatterns));
    }

    [TestMethod]
    public void NewsletterTypeGroups_MaintainUpperPushPullLower_HasAllMovementPatterns()
    {
        var groups = new NewsletterTypeGroups(StrengtheningPreference.Maintain, Frequency.PushPullLeg3Day);
        Assert.IsTrue(groups.Aggregate((MovementPattern)0, (curr, n) => curr | n.MovementPatterns).HasFlag(AllMovementPatterns));
    }
}