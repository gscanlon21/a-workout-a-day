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
    private readonly StrengtheningPreference StrengtheningPreference;
    private readonly Frequency Frequency;

    public NewsletterTypeGroups(StrengtheningPreference preference, Frequency frequency)
    {
        StrengtheningPreference = preference;
        Frequency = frequency;
    }

    private IntensityLevel GetIntensityLevelFromPreference()
    {
        return StrengtheningPreference switch
        {
            StrengtheningPreference.Maintain => IntensityLevel.Maintain,
            StrengtheningPreference.Obtain => IntensityLevel.Obtain,
            StrengtheningPreference.Gain => IntensityLevel.Gain,
            _ => IntensityLevel.Maintain
        };
    }

    private IEnumerator<NewsletterRotation> GetFullBody2DayRotation()
    {
        // Full-body
        yield return new NewsletterRotation(1, NewsletterType.Strength, GetIntensityLevelFromPreference(),
            MuscleGroups.UpperLower, 
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Squat | MovementPattern.Lunge | MovementPattern.Rotation);

        // Full-body
        yield return new NewsletterRotation(2, NewsletterType.Strength, GetIntensityLevelFromPreference(),
            MuscleGroups.UpperLower,
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.HipHinge | MovementPattern.Carry | MovementPattern.Rotation);
    }

    private IEnumerator<NewsletterRotation> GetUpperLower4DayRotation()
    {
        // Upper body push
        yield return new NewsletterRotation(1, NewsletterType.Strength, GetIntensityLevelFromPreference(), 
            MuscleGroups.UpperBody, 
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Rotation);

        // Lower body
        yield return new NewsletterRotation(2, NewsletterType.Strength, GetIntensityLevelFromPreference(), 
            MuscleGroups.LowerBody, 
            MovementPattern.Squat | MovementPattern.Lunge);

        // Upper body pull
        yield return new NewsletterRotation(3, NewsletterType.Strength, GetIntensityLevelFromPreference(), 
            MuscleGroups.UpperBody, 
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Rotation);

        // Lower body alt
        yield return new NewsletterRotation(4, NewsletterType.Strength, GetIntensityLevelFromPreference(), 
            MuscleGroups.LowerBody, 
            MovementPattern.HipHinge | MovementPattern.Carry);
    }

    private IEnumerator<NewsletterRotation> GetUpperLower2DayRotation()
    {
        // Upper body
        yield return new NewsletterRotation(1, NewsletterType.Strength, GetIntensityLevelFromPreference(),
            MuscleGroups.UpperBody,
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Rotation);

        // Lower body
        yield return new NewsletterRotation(2, NewsletterType.Strength, GetIntensityLevelFromPreference(),
            MuscleGroups.LowerBody,
            MovementPattern.Squat | MovementPattern.Lunge | MovementPattern.HipHinge | MovementPattern.Carry);
    }

    private IEnumerator<NewsletterRotation> GetPushPullLeg3DayRotation()
    {
        // What day to work rotator cuffs and forearms?

        // Upper body push
        yield return new NewsletterRotation(1, NewsletterType.Strength, GetIntensityLevelFromPreference(),
            MuscleGroups.UpperBodyPush,
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Rotation);

        // Upper body pull
        yield return new NewsletterRotation(2, NewsletterType.Strength, GetIntensityLevelFromPreference(),
            MuscleGroups.UpperBodyPull,
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Rotation);

        // Lower body
        yield return new NewsletterRotation(3, NewsletterType.Strength, GetIntensityLevelFromPreference(),
            MuscleGroups.LowerBody,
            MovementPattern.Squat | MovementPattern.Lunge | MovementPattern.HipHinge | MovementPattern.Carry);
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
