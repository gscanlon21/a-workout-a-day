using Data.Entities.User;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User.Components;

public class AdvancedViewModel
{
    [Obsolete("Public parameterless constructor for model binding.", error: true)]
    public AdvancedViewModel() { }

    public AdvancedViewModel(Data.Entities.User.User user, UserPreference userPreference, string token)
    {
        Email = user.Email;
        Token = token;

        AtLeastXUniqueMusclesPerExercise_Accessory = userPreference.AtLeastXUniqueMusclesPerExercise_Accessory;
        AtLeastXUniqueMusclesPerExercise_Flexibility = userPreference.AtLeastXUniqueMusclesPerExercise_Flexibility;
        AtLeastXUniqueMusclesPerExercise_Mobility = userPreference.AtLeastXUniqueMusclesPerExercise_Mobility;
        WeightSecondaryMusclesXTimesLess = userPreference.WeightSecondaryMusclesXTimesLess;
        WeightIsolationXTimesMore = userPreference.WeightIsolationXTimesMore;
        IgnorePrerequisites = userPreference.IgnorePrerequisites;
    }

    public bool IsNotDefault => IgnorePrerequisites != false
        || AtLeastXUniqueMusclesPerExercise_Accessory != UserPreference.Consts.AtLeastXUniqueMusclesPerExercise_AccessoryDefault
        || AtLeastXUniqueMusclesPerExercise_Flexibility != UserPreference.Consts.AtLeastXUniqueMusclesPerExercise_FlexibilityDefault
        || AtLeastXUniqueMusclesPerExercise_Mobility != UserPreference.Consts.AtLeastXUniqueMusclesPerExercise_MobilityDefault
        || WeightIsolationXTimesMore != UserPreference.Consts.WeightIsolationXTimesMoreDefault
        || WeightSecondaryMusclesXTimesLess != UserPreference.Consts.WeightSecondaryMusclesXTimesLessDefault;

    public string Token { get; init; } = null!;
    public string Email { get; init; } = null!;

    [Display(Name = "Ignore Prerequisites", Description = "Skip checking prerequisite exercises when building workouts.")]
    public bool IgnorePrerequisites { get; set; }

    [Display(Name = "At Least X Unique Muscles Per Exercise (Mobility)", Description = "A higher value will result in shorter warmup sections and decreased exercise variety.")]
    [Range(UserPreference.Consts.AtLeastXUniqueMusclesPerExercise_MobilityMin, UserPreference.Consts.AtLeastXUniqueMusclesPerExercise_MobilityMax)]
    public int AtLeastXUniqueMusclesPerExercise_Mobility { get; set; }

    [Display(Name = "At Least X Unique Muscles Per Exercise (Flexibility)", Description = "A higher value will result in shorter cooldown sections and decreased exercise variety.")]
    [Range(UserPreference.Consts.AtLeastXUniqueMusclesPerExercise_FlexibilityMin, UserPreference.Consts.AtLeastXUniqueMusclesPerExercise_FlexibilityMax)]
    public int AtLeastXUniqueMusclesPerExercise_Flexibility { get; set; }

    [Display(Name = "At Least X Unique Muscles Per Exercise (Accessory)", Description = "A higher value will result in shorter accessory sections and decreased exercise variety.")]
    [Range(UserPreference.Consts.AtLeastXUniqueMusclesPerExercise_AccessoryMin, UserPreference.Consts.AtLeastXUniqueMusclesPerExercise_AccessoryMax)]
    public int AtLeastXUniqueMusclesPerExercise_Accessory { get; set; }

    [Display(Name = "Weight Secondary Muscles X Times Less", Description = "Changes how secondary muscles are weighted in the weekly muscle targets graph.")]
    [Range(UserPreference.Consts.WeightSecondaryMusclesXTimesLessMin, UserPreference.Consts.WeightSecondaryMusclesXTimesLessMax)]
    public double WeightSecondaryMusclesXTimesLess { get; set; }

    [Display(Name = "Weight Isolation Exercises X Times More", Description = "Changes how secondary muscles are weighted in the weekly muscle targets graph.")]
    [Range(UserPreference.Consts.WeightIsolationXTimesMoreMin, UserPreference.Consts.WeightIsolationXTimesMoreMax)]
    public double WeightIsolationXTimesMore { get; set; }
}
