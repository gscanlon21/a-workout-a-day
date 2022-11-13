using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Extensions;

namespace Web.Test
{
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
            var groups = new NewsletterTypeGroups(StrengtheningPreference.Maintain, Frequency.FullBody);
            Assert.IsTrue(groups.Aggregate((MovementPattern)0, (curr, n) => curr | n.MovementPatterns).HasFlag(AllMovementPatterns));
        }

        [TestMethod]
        public void NewsletterTypeGroups_MaintainUpperLower_HasAllMovementPatterns()
        {
            var groups = new NewsletterTypeGroups(StrengtheningPreference.Maintain, Frequency.UpperLowerBodySplit);
            Assert.IsTrue(groups.Aggregate((MovementPattern)0, (curr, n) => curr | n.MovementPatterns).HasFlag(AllMovementPatterns));
        }
    }
}