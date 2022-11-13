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

    public IEnumerator<NewsletterRotation> GetEnumerator()
    {
        yield return new NewsletterRotation(1, NewsletterType.Strength, GetIntensityLevelFromPreference(), Frequency switch
        {
            Frequency.FullBody => MuscleGroups.All,
            Frequency.UpperLowerBodySplit => MuscleGroups.UpperBody,
            _ => MuscleGroups.All
        }, Frequency switch
        {
            Frequency.FullBody => MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Squat | MovementPattern.Lunge,
            Frequency.UpperLowerBodySplit => MovementPattern.HorizontalPush | MovementPattern.VerticalPush,
            _ => MovementPattern.None
        });


        yield return new NewsletterRotation(2, NewsletterType.Strength, GetIntensityLevelFromPreference(), Frequency switch
        {
            Frequency.FullBody => MuscleGroups.All,
            Frequency.UpperLowerBodySplit => MuscleGroups.LowerBody,
            _ => MuscleGroups.All
        }, Frequency switch
        {
            Frequency.FullBody => MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Carry | MovementPattern.HipHinge,
            Frequency.UpperLowerBodySplit => MovementPattern.Squat | MovementPattern.Lunge,
            _ => MovementPattern.None
        });


        yield return new NewsletterRotation(3, NewsletterType.Stability, GetIntensityLevelFromPreference(), MuscleGroups.All, MovementPattern.Rotation);


        yield return new NewsletterRotation(4, NewsletterType.Strength, GetIntensityLevelFromPreference(), Frequency switch
        {
            Frequency.FullBody => MuscleGroups.All,
            Frequency.UpperLowerBodySplit => MuscleGroups.UpperBody,
            _ => MuscleGroups.All
        }, Frequency switch
        {
            Frequency.FullBody => MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Squat | MovementPattern.Lunge,
            Frequency.UpperLowerBodySplit => MovementPattern.HorizontalPull | MovementPattern.VerticalPull,
            _ => MovementPattern.None
        });


        yield return new NewsletterRotation(5, NewsletterType.Strength, GetIntensityLevelFromPreference(), Frequency switch
        {
            Frequency.FullBody => MuscleGroups.All,
            Frequency.UpperLowerBodySplit => MuscleGroups.LowerBody,
            _ => MuscleGroups.All
        }, Frequency switch
        {
            Frequency.FullBody => MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Carry | MovementPattern.HipHinge,
            Frequency.UpperLowerBodySplit => MovementPattern.HipHinge | MovementPattern.Carry,
            _ => MovementPattern.None
        });
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
