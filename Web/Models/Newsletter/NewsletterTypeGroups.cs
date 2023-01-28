using Web.Entities.Exercise;
using Web.Entities.Newsletter;
using Web.Models.Exercise;
using Web.Models.User;
using System.Collections;

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

    private IEnumerator<NewsletterRotation> GetFullBody2DayRotation()
    {
        yield return new NewsletterRotation(1,
            MuscleGroups.UpperLower,
            MovementPattern.Push | MovementPattern.KneeFlexion | MovementPattern.Rotation);

        yield return new NewsletterRotation(2,
            MuscleGroups.UpperLower,
            MovementPattern.Pull | MovementPattern.HipExtension | MovementPattern.Carry);
    }

    private IEnumerator<NewsletterRotation> GetUpperLower4DayRotation()
    {
        yield return new NewsletterRotation(1,
            MuscleGroups.UpperBody, 
            MovementPattern.HorizontalPush | MovementPattern.HorizontalPull | MovementPattern.Rotation);

        yield return new NewsletterRotation(2,
            MuscleGroups.LowerBody, 
            MovementPattern.Squat | MovementPattern.Lunge);

        yield return new NewsletterRotation(3,
            MuscleGroups.UpperBody,
            MovementPattern.VerticalPush | MovementPattern.VerticalPull | MovementPattern.Rotation);

        yield return new NewsletterRotation(4,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Carry);
    }

    private IEnumerator<NewsletterRotation> GetUpperLower2DayRotation()
    {
        yield return new NewsletterRotation(1,
            MuscleGroups.UpperBody,
            MovementPattern.Push | MovementPattern.Pull | MovementPattern.Rotation);

        yield return new NewsletterRotation(2,
            MuscleGroups.LowerBody,
            MovementPattern.KneeFlexion | MovementPattern.HipExtension | MovementPattern.Carry);
    }

    private IEnumerator<NewsletterRotation> GetPushPullLeg3DayRotation()
    {
        yield return new NewsletterRotation(1,
            MuscleGroups.UpperBodyPush,
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Rotation);

        yield return new NewsletterRotation(2,
            MuscleGroups.UpperBodyPull,
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Rotation);

        yield return new NewsletterRotation(3, 
            MuscleGroups.LowerBody,
            MovementPattern.KneeFlexion | MovementPattern.HipExtension | MovementPattern.Carry);
    }

    public IEnumerator<NewsletterRotation> GetEnumerator()
    {
        return Frequency switch
        {
            Frequency.FullBody2Day => GetFullBody2DayRotation(),
            Frequency.UpperLowerBodySplit4Day => GetUpperLower4DayRotation(),
            Frequency.UpperLowerBodySplit2Day => GetUpperLower2DayRotation(),
            Frequency.PushPullLeg3Day => GetPushPullLeg3DayRotation(),
            _ => throw new NotImplementedException()
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
