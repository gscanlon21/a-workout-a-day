using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User.Components;

public class AdvancedViewModel
{
    [Obsolete("Public parameterless constructor for model binding.", error: true)]
    public AdvancedViewModel() { }

    public AdvancedViewModel(Data.Entities.User.User user, string token)
    {
        Token = token;
        Email = user.Email;
        IsNewToFitness = user.IsNewToFitness;

        IgnorePrerequisites = user.IgnorePrerequisites;
        AtLeastXUniqueMusclesPerExercise_Accessory = user.AtLeastXUniqueMusclesPerExercise_Accessory;
        AtLeastXUniqueMusclesPerExercise_Flexibility = user.AtLeastXUniqueMusclesPerExercise_Flexibility;
        AtLeastXUniqueMusclesPerExercise_Mobility = user.AtLeastXUniqueMusclesPerExercise_Mobility;
        WeightSecondaryMusclesXTimesLess = user.WeightSecondaryMusclesXTimesLess;
        WeightCoreExercisesXTimesMore = user.WeightCoreExercisesXTimesMore;
        WeightIsolationXTimesMore = user.WeightIsolationXTimesMore;
        FootnoteCountTop = user.FootnoteCountTop;
        FootnoteCountBottom = user.FootnoteCountBottom;
    }

    public bool IsNotDefault => IgnorePrerequisites != false
        || AtLeastXUniqueMusclesPerExercise_Accessory != Data.Entities.User.User.Consts.AtLeastXUniqueMusclesPerExercise_AccessoryDefault
        || AtLeastXUniqueMusclesPerExercise_Flexibility != Data.Entities.User.User.Consts.AtLeastXUniqueMusclesPerExercise_FlexibilityDefault
        || AtLeastXUniqueMusclesPerExercise_Mobility != Data.Entities.User.User.Consts.AtLeastXUniqueMusclesPerExercise_MobilityDefault
        || WeightIsolationXTimesMore != Data.Entities.User.User.Consts.WeightIsolationXTimesMoreDefault
        || WeightCoreExercisesXTimesMore != Data.Entities.User.User.Consts.WeightCoreExercisesXTimesMoreDefault
        || WeightSecondaryMusclesXTimesLess != Data.Entities.User.User.Consts.WeightSecondaryMusclesXTimesLessDefault
        || FootnoteCountTop != Data.Entities.User.User.Consts.FootnoteCountTopDefault
        || FootnoteCountBottom != Data.Entities.User.User.Consts.FootnoteCountBottomDefault;

    public bool IsNewToFitness { get; init; }
    public string Token { get; init; } = null!;
    public string Email { get; init; } = null!;

    [Display(Name = "Number of Footnotes (Top)")]
    public int FootnoteCountTop { get; set; }

    [Display(Name = "Number of Footnotes (Bottom)")]
    public int FootnoteCountBottom { get; set; }

    [Display(Name = "Ignore Prerequisites", Description = "Skip checking prerequisite exercises when building workouts.")]
    public bool IgnorePrerequisites { get; set; }

    [Display(Name = "At Least X Unique Muscles Per Exercise (Mobility)", Description = "A higher value will result in shorter warmup sections and decreased exercise variety.")]
    [Range(Data.Entities.User.User.Consts.AtLeastXUniqueMusclesPerExercise_MobilityMin, Data.Entities.User.User.Consts.AtLeastXUniqueMusclesPerExercise_MobilityMax)]
    public int AtLeastXUniqueMusclesPerExercise_Mobility { get; set; }

    [Display(Name = "At Least X Unique Muscles Per Exercise (Flexibility)", Description = "A higher value will result in shorter cooldown sections and decreased exercise variety.")]
    [Range(Data.Entities.User.User.Consts.AtLeastXUniqueMusclesPerExercise_FlexibilityMin, Data.Entities.User.User.Consts.AtLeastXUniqueMusclesPerExercise_FlexibilityMax)]
    public int AtLeastXUniqueMusclesPerExercise_Flexibility { get; set; }

    [Display(Name = "At Least X Unique Muscles Per Exercise (Accessory)", Description = "A higher value will result in shorter accessory sections and decreased exercise variety.")]
    [Range(Data.Entities.User.User.Consts.AtLeastXUniqueMusclesPerExercise_AccessoryMin, Data.Entities.User.User.Consts.AtLeastXUniqueMusclesPerExercise_AccessoryMax)]
    public int AtLeastXUniqueMusclesPerExercise_Accessory { get; set; }

    [Display(Name = "Weight Secondary Muscles X Times Less", Description = "Changes how secondary muscles are weighted in the weekly muscle targets graph.")]
    [Range(Data.Entities.User.User.Consts.WeightSecondaryMusclesXTimesLessMin, Data.Entities.User.User.Consts.WeightSecondaryMusclesXTimesLessMax)]
    public double WeightSecondaryMusclesXTimesLess { get; set; }

    [Display(Name = "Weight Isolation Exercises X Times More", Description = "Changes how isolation exercises are weighted in the weekly muscle targets graph.")]
    [Range(Data.Entities.User.User.Consts.WeightIsolationXTimesMoreMin, Data.Entities.User.User.Consts.WeightIsolationXTimesMoreMax)]
    public double WeightIsolationXTimesMore { get; set; }

    [Display(Name = "Weight Core Exercises X Times More", Description = "Changes the frequency of when core exercises are picked relative to more obscure exercises.")]
    [Range(Data.Entities.User.User.Consts.WeightCoreExercisesXTimesMoreMin, Data.Entities.User.User.Consts.WeightCoreExercisesXTimesMoreMax)]
    public double WeightCoreExercisesXTimesMore { get; set; }
}
