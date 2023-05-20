using System.Collections;
using Web.Entities.Newsletter;
using Web.Models.Exercise;
using Web.Models.User;

namespace Web.Models.Newsletter;

/// <summary>
/// The ~weekly routine of exercise types for each strengthing preference.
/// </summary>
public class NewsletterTypeGroups : IEnumerable<NewsletterRotation>
{
    private readonly Frequency Frequency;

    public NewsletterTypeGroups(Frequency frequency)
    {
        Frequency = frequency;
    }

    /// <summary>
    /// Off-day mobility/stretching rotation.
    /// </summary>
    private static IEnumerator<NewsletterRotation> GetOffDayStretchingRotation()
    {
        yield return new NewsletterRotation(1, MuscleGroups.UpperLower, MovementPattern.None);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    private static IEnumerator<NewsletterRotation> GetFullBody2DayRotation()
    {
        yield return new NewsletterRotation(1,
            MuscleGroups.UpperLower,
            MovementPattern.HorizontalPush | MovementPattern.HorizontalPull | MovementPattern.Squat | MovementPattern.Lunge | MovementPattern.Rotation);

        yield return new NewsletterRotation(2,
            MuscleGroups.UpperLower,
            MovementPattern.VerticalPush | MovementPattern.VerticalPull | MovementPattern.KneeFlexion | MovementPattern.HipExtension | MovementPattern.Carry);
    }

    /// <summary>
    /// An implementation of the Push/Pull/Legs workout split.
    /// </summary>
    private static IEnumerator<NewsletterRotation> GetPushPullLeg3DayRotation()
    {
        // Lower body first. The workouts are better w/o equipment, which tends to happen when someone signs up w/o selecting their equipment before the first workout arrives.
        yield return new NewsletterRotation(1,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat | MovementPattern.Lunge);

        yield return new NewsletterRotation(2,
            MuscleGroups.UpperBodyPush,
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Rotation);

        yield return new NewsletterRotation(3,
            MuscleGroups.UpperBodyPull,
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Carry);
    }

    /// <summary>
    /// An implementation of the Upper/Lower Body workout split.
    /// </summary>
    private static IEnumerator<NewsletterRotation> GetUpperLower4DayRotation()
    {
        // Lower body first. The workouts are better w/o equipment, which tends to happen when someone signs up w/o selecting their equipment before the first workout arrives.
        yield return new NewsletterRotation(1,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat | MovementPattern.Lunge);

        yield return new NewsletterRotation(2,
            MuscleGroups.UpperBody,
            MovementPattern.HorizontalPush | MovementPattern.HorizontalPull | MovementPattern.Rotation);

        yield return new NewsletterRotation(3,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat | MovementPattern.Lunge);

        yield return new NewsletterRotation(4,
            MuscleGroups.UpperBody,
            MovementPattern.VerticalPush | MovementPattern.VerticalPull | MovementPattern.Carry);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    private static IEnumerator<NewsletterRotation> GetUpperLowerFullBody3DayRotation()
    {
        // Lower body first. The workouts are better w/o equipment, which tends to happen when someone signs up w/o selecting their equipment before the first workout arrives.
        yield return new NewsletterRotation(1,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat | MovementPattern.Lunge);

        yield return new NewsletterRotation(2,
            MuscleGroups.UpperBody,
            MovementPattern.HorizontalPush | MovementPattern.HorizontalPull | MovementPattern.Rotation);

        yield return new NewsletterRotation(3,
            MuscleGroups.UpperLower,
            MovementPattern.VerticalPush | MovementPattern.VerticalPull | MovementPattern.KneeFlexion | MovementPattern.HipExtension | MovementPattern.Carry);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    private static IEnumerator<NewsletterRotation> GetPushPullLegsFullBody4DayRotation()
    {
        // Lower body first. The workouts are better w/o equipment, which tends to happen when someone signs up w/o selecting their equipment before the first workout arrives.
        yield return new NewsletterRotation(1,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat | MovementPattern.Lunge);

        yield return new NewsletterRotation(2,
            MuscleGroups.UpperBodyPush,
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Carry);

        yield return new NewsletterRotation(3,
            MuscleGroups.UpperBodyPull,
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Rotation);

        yield return new NewsletterRotation(4,
            MuscleGroups.UpperLower,
            MovementPattern.Squat | MovementPattern.Lunge | MovementPattern.HipExtension);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    private static IEnumerator<NewsletterRotation> GetPushPullLegsUpperLower5DayRotation()
    {
        // Lower body first. The workouts are better w/o equipment, which tends to happen when someone signs up w/o selecting their equipment before the first workout arrives.
        yield return new NewsletterRotation(1,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat | MovementPattern.Lunge);

        yield return new NewsletterRotation(2,
            MuscleGroups.UpperBodyPush,
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Carry);

        yield return new NewsletterRotation(3,
            MuscleGroups.UpperBodyPull,
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Rotation);

        yield return new NewsletterRotation(4,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat | MovementPattern.Lunge);

        yield return new NewsletterRotation(5,
            MuscleGroups.UpperBody,
            MovementPattern.Carry | MovementPattern.Rotation);
    }

    public IEnumerator<NewsletterRotation> GetEnumerator()
    {
        return Frequency switch
        {
            Frequency.FullBody2Day => GetFullBody2DayRotation(),
            Frequency.PushPullLeg3Day => GetPushPullLeg3DayRotation(),
            Frequency.UpperLowerBodySplit4Day => GetUpperLower4DayRotation(),
            Frequency.UpperLowerFullBodySplit3Day => GetUpperLowerFullBody3DayRotation(),
            Frequency.PushPullLegsFullBodySplit4Day => GetPushPullLegsFullBody4DayRotation(),
            Frequency.PushPullLegsUpperLowerSplit5Day => GetPushPullLegsUpperLower5DayRotation(),
            Frequency.OffDayStretches => GetOffDayStretchingRotation(),
            _ => throw new NotImplementedException()
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
