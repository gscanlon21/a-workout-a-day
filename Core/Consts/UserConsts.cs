﻿using Core.Models.Equipment;
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

    public const int UserSetsMin = 0;
    public const int UserSetsDefault = 0;
    public const int UserSetsMax = 10;

    public const int UserRepsMin = 0;
    public const int UserRepsDefault = 0;
    public const int UserRepsMax = 30;

    public const int UserSecsMin = 0;
    public const int UserSecsDefault = 0;
    public const int UserSecsMax = 600;

    public const int UserWeightMin = 0;
    public const int UserWeightDefault = 0;
    public const int UserWeightMax = 900;

    public const int UserMuscleMobilityMin = 0;
    public const int UserMuscleMobilityMax = 3;

    public const int UserMuscleFlexibilityMin = 0;
    public const int UserMuscleFlexibilityMax = 3;

    public const int FootnoteCountMin = 1;
    public const int FootnoteCountTopDefault = 2;
    public const int FootnoteCountBottomDefault = 2;
    public const int FootnoteCountMax = 4;

    public const int AtLeastXUniqueMusclesPerExercise_MobilityMin = 1;
    public const int AtLeastXUniqueMusclesPerExercise_MobilityDefault = 3;
    public const int AtLeastXUniqueMusclesPerExercise_MobilityMax = 4;

    public const int AtLeastXUniqueMusclesPerExercise_AccessoryMin = 1;
    public const int AtLeastXUniqueMusclesPerExercise_AccessoryDefault = 3;
    public const int AtLeastXUniqueMusclesPerExercise_AccessoryMax = 4;

    public const int AtLeastXUniqueMusclesPerExercise_FlexibilityMin = 1;
    public const int AtLeastXUniqueMusclesPerExercise_FlexibilityDefault = 2;
    public const int AtLeastXUniqueMusclesPerExercise_FlexibilityMax = 4;

    public const int PrehabCountMin = 1;
    public const int PrehabCountDefault = 1;
    public const int PrehabCountMax = 6;

    /// <summary>
    /// Max number of strengthening muscles worked to count as an isolation exercise.
    /// </summary>
    public const double IsolationIsXStrengthMuscles = 2;
    public const double WeightIsolationXTimesMoreMin = 1;
    public const double WeightIsolationXTimesMoreDefault = 2;
    public const double WeightIsolationXTimesMoreMax = 3;

    public const double WeightSecondaryMusclesXTimesLessMin = 2;
    public const double WeightSecondaryMusclesXTimesLessDefault = 3;
    public const double WeightSecondaryMusclesXTimesLessMax = 4;

    /// <summary>
    /// Set all days as strengthening days so backfill works. 
    /// The user can change their preferences later.
    /// </summary>
    public const Days SendDaysDefault = Days.All;

    /// <summary>
    /// Set this as the full body workout split so backfill has less workouts to create.
    /// The user can change their preferences later.
    /// </summary>
    public const Frequency FrequencyDefault = Frequency.FullBody2Day;

    public const Intensity IntensityDefault = Intensity.Light;

    public const Equipment EquipmentDefault = Equipment.Dumbbells;

    public const ImageType ImageTypeDefault = ImageType.Animated;

    public const Verbosity VerbosityDefault = Verbosity.StrengthMuscles | Verbosity.StretchMuscles | Verbosity.Instructions | Verbosity.ProgressionBar;

    public const FootnoteType FootnotesDefault = FootnoteType.FitnessTips | FootnoteType.FitnessFacts
        | FootnoteType.HealthTips | FootnoteType.HealthFacts | FootnoteType.GoodVibes | FootnoteType.Mindfulness;

    /// <summary>
    /// This shouldn't be too high (>12) or else the program will spend too much time trying 
    /// to get the user in range and end up not working or overworking specific muscles in the interim.
    /// 
    /// This shouldn't be too low (<12) or else the muscle target value will drop too much
    /// during rest days and overwork the user the next time they see a workout.
    /// 
    /// Using half of the TargetVolumePerExercise (24) so 1 exercise / week increments up 2.
    /// </summary>
    public const int TrainingVolumeWeeks = 12;

    /// <summary>
    /// After how many weeks until muscle targets start taking effect.
    /// </summary>
    public const int MuscleTargetsTakeEffectAfterXWeeks = 1;

    /// <summary>
    /// Sections that are used in the calculation of muscle targets.
    /// </summary>
    public const Section MuscleTargetSections = Section.Functional | Section.Accessory | Section.Core;

    /// <summary>
    /// The lowest the user's progression can go.
    /// </summary>
    public const int MinUserProgression = 5;

    /// <summary>
    /// Also the user's starting progression when the user is new to fitness.
    /// </summary>
    public const int UserIsNewProgression = 25;

    /// <summary>
    /// Also the user's starting progression when the user is not new to fitness.
    /// </summary>
    public const int UserIsSeasonedProgression = 50;

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

    /// <summary>
    /// How many months until the user's weight logs are deleted.
    /// </summary>
    public const int DeleteWeightsAfterXMonths = 60;
}
