using FinerFettle.Web.Entities.Exercise;
using FinerFettle.Web.Entities.Newsletter;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;
using System.Collections;

namespace FinerFettle.Web.Models.Newsletter;

/// <summary>
/// The ~weekly routine of exercise types for each strengthing preference.
/// </summary>
public class NewsletterTypeGroups : IEnumerable<NewsletterRotation>
{
    private readonly StrengtheningPreference StrengtheningPreference;

    public NewsletterTypeGroups(StrengtheningPreference preference)
    {
        StrengtheningPreference = preference;
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
        yield return new NewsletterRotation(1, NewsletterType.Strength, GetIntensityLevelFromPreference(), StrengtheningPreference switch
        {
            StrengtheningPreference.Maintain => MuscleGroups.All,
            StrengtheningPreference.Obtain => MuscleGroups.UpperBody,
            StrengtheningPreference.Gain => MuscleGroups.UpperBody,
            _ => MuscleGroups.All
        });

        yield return new NewsletterRotation(2, NewsletterType.Strength, GetIntensityLevelFromPreference(), StrengtheningPreference switch
        {
            StrengtheningPreference.Maintain => MuscleGroups.All,
            StrengtheningPreference.Obtain => MuscleGroups.LowerBody,
            StrengtheningPreference.Gain => MuscleGroups.LowerBody,
            _ => MuscleGroups.All
        });

        yield return new NewsletterRotation(3, NewsletterType.Stability, GetIntensityLevelFromPreference(), MuscleGroups.All);

        yield return new NewsletterRotation(4, NewsletterType.Strength, GetIntensityLevelFromPreference(), StrengtheningPreference switch
        {
            StrengtheningPreference.Maintain => MuscleGroups.All,
            StrengtheningPreference.Obtain => MuscleGroups.All,
            StrengtheningPreference.Gain => MuscleGroups.UpperBody,
            _ => MuscleGroups.All
        });

        yield return new NewsletterRotation(5, NewsletterType.Strength, GetIntensityLevelFromPreference(), StrengtheningPreference switch
        {
            StrengtheningPreference.Maintain => MuscleGroups.All,
            StrengtheningPreference.Obtain => MuscleGroups.All,
            StrengtheningPreference.Gain => MuscleGroups.LowerBody,
            _ => MuscleGroups.All
        });

        //yield return new NewsletterRotation(6, ExerciseType.Strength, IntensityLevel.Endurance, MuscleGroups.UpperBody);
        //yield return new NewsletterRotation(7, ExerciseType.Strength, IntensityLevel.Endurance, MuscleGroups.LowerBody);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
