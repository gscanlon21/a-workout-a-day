using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;

namespace Core.Consts;

/// <summary>
/// Shared user consts for Functions and Web.
/// </summary>
public class UserConsts
{
    public const string DemoUser = "demo@aworkoutaday.com";
    public const string DemoToken = "00000000-0000-0000-0000-000000000000";

    public const int DeloadAfterXWeeksMin = 3;
    public const int DeloadAfterXWeeksDefault = 9;
    public const int DeloadAfterXWeeksMax = 15;

    public const int LagRefreshXWeeksMin = 0;
    public const int LagRefreshXWeeksDefault = 0;
    public const int LagRefreshXWeeksMax = 12;

    public const int PadRefreshXWeeksMin = 0;
    public const int PadRefreshXWeeksDefault = 0;
    public const int PadRefreshXWeeksMax = 12;

    public const int SendHourMin = 0;
    public const int SendHourDefault = 0;
    public const int SendHourMax = 23;

    public const int UserWeightMin = 0;
    public const int UserWeightDefault = 0;
    public const int UserWeightMax = 999;

    public const int UserSetsMin = 0;
    public const int UserSetsDefault = 0;
    public const int UserSetsMax = 30;

    public const int UserRepsMin = 0;
    public const int UserRepsDefault = 0;
    public const int UserRepsMax = 300;

    public const int UserMuscleMobilityMin = 0;
    public const int UserMuscleMobilityMax = 3;

    public const int UserMuscleFlexibilityMin = 0;
    public const int UserMuscleFlexibilityMax = 3;

    public const int FootnoteCountMin = 1;
    public const int FootnoteCountTopDefault = 2;
    public const int FootnoteCountBottomDefault = 2;
    public const int FootnoteCountMax = 4;

    public const int AtLeastXUniqueMusclesPerExercise_FlexibilityMin = 1;
    public const int AtLeastXUniqueMusclesPerExercise_FlexibilityDefault = 3;
    public const int AtLeastXUniqueMusclesPerExercise_FlexibilityMax = 4;

    public const int AtLeastXUniqueMusclesPerExercise_MobilityMin = 1;
    public const int AtLeastXUniqueMusclesPerExercise_MobilityDefault = 3;
    public const int AtLeastXUniqueMusclesPerExercise_MobilityMax = 4;

    public const int AtLeastXUniqueMusclesPerExercise_AccessoryMin = 1;
    public const int AtLeastXUniqueMusclesPerExercise_AccessoryDefault = 3;
    public const int AtLeastXUniqueMusclesPerExercise_AccessoryMax = 4;

    public const double WeightIsolationXTimesMoreMin = 1;
    public const double WeightIsolationXTimesMoreDefault = 1.5;
    public const double WeightIsolationXTimesMoreMax = 2;

    public const double WeightSecondaryMusclesXTimesLessMin = 2;
    public const double WeightSecondaryMusclesXTimesLessDefault = 3;
    public const double WeightSecondaryMusclesXTimesLessMax = 4;

    public const Days DaysDefault = Days.Monday | Days.Tuesday | Days.Thursday | Days.Friday;

    public const Frequency FrequencyDefault = Frequency.UpperLowerBodySplit4Day;

    public const Intensity IntensityDefault = Intensity.Light;

    public const Verbosity VerbosityDefault = Verbosity.Instructions
        | Verbosity.Images | Verbosity.ProgressionBar | Verbosity.Proficiency;

    public const FootnoteType FootnotesDefault = FootnoteType.FitnessTips | FootnoteType.FitnessFacts
        | FootnoteType.HealthTips | FootnoteType.HealthFacts | FootnoteType.GoodVibes | FootnoteType.Mindfulness;

    /// <summary>
    /// This shouldn't be too high (>8) or else the program will spend too much time trying 
    /// to get the user in range and end up not working or overworking specific muscles in the interim.
    /// 
    /// This shouldn't be too low (<8) or else the muscle target value will drop too much
    /// during rest days and overwork the user the next time they see a workout.
    /// </summary>
    public const int TrainingVolumeWeeks = 8;

    /// <summary>
    /// 8 because we want to leave the user with at least one week of data 
    /// and muscle targets only take effect after 1 week (MuscleTargetsTakeEffectAfterXWeeks).
    /// </summary>
    public const int TrainingVolumeClearDays = 8;

    /// <summary>
    /// After how many weeks until muscle targets start taking effect.
    /// </summary>
    public const int MuscleTargetsTakeEffectAfterXWeeks = 1;

    /// <summary>
    /// The lowest the user's progression can go.
    /// 
    /// Also the user's starting progression when the user is new to fitness.
    /// </summary>
    public const int MinUserProgression = 5;

    /// <summary>
    /// Also the user's starting progression when the user is not new to fitness.
    /// </summary>
    public const int MidUserProgression = 50;

    /// <summary>
    /// The highest the user's progression can go.
    /// </summary>
    public const int MaxUserProgression = 95;

    /// <summary>
    /// How many custom user_frequency records do we allow per user?
    /// </summary>
    public const int MaxUserFrequencies = 7;

    /// <summary>
    /// How much to increment the user_muscle_strength target ranges with each increment?
    /// </summary>
    public const int IncrementMuscleTargetBy = 10;

    /// <summary>
    /// How many months until the user's account is disabled for inactivity.
    /// </summary>
    public const int DisableAfterXMonths = 3;

    /// <summary>
    /// How many months until the user's account is deleted for inactivity.
    /// </summary>
    public const int DeleteAfterXMonths = 6;

    /// <summary>
    /// How many months until the user's newsletter logs are deleted.
    /// </summary>
    public const int DeleteLogsAfterXMonths = 12;
}
