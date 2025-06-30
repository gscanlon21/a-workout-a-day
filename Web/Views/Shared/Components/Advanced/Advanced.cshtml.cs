using System.ComponentModel.DataAnnotations;

namespace Web.Views.Shared.Components.Advanced;

public class AdvancedViewModel
{
    [Obsolete("Public parameterless constructor required for model binding.", error: true)]
    public AdvancedViewModel() { }

    public AdvancedViewModel(Data.Entities.User.User user, string token)
    {
        Token = token;
        Email = user.Email;
        IsNewToFitness = user.IsNewToFitness;

        ExtendedWarmup = user.ExtendedWarmup;
        IgnorePrerequisites = user.IgnorePrerequisites;
        AtLeastXUniqueMusclesPerExercise_Mobility = user.AtLeastXUniqueMusclesPerExercise_Mobility;
        AtLeastXUniqueMusclesPerExercise_Accessory = user.AtLeastXUniqueMusclesPerExercise_Accessory;
        AtLeastXUniqueMusclesPerExercise_Flexibility = user.AtLeastXUniqueMusclesPerExercise_Flexibility;
        WeightSecondaryXTimesLess = user.WeightSecondaryXTimesLess;
        WeightIsolationXTimesMore = user.WeightIsolationXTimesMore;
        FootnoteCountBottom = user.FootnoteCountBottom;
        FootnoteCountTop = user.FootnoteCountTop;
    }

    public bool IsNotDefault => ExtendedWarmup != false || IgnorePrerequisites != false
        || AtLeastXUniqueMusclesPerExercise_Mobility != UserConsts.AtLeastXUniqueMusclesPerExercise_MobilityDefault
        || AtLeastXUniqueMusclesPerExercise_Accessory != UserConsts.AtLeastXUniqueMusclesPerExercise_AccessoryDefault
        || AtLeastXUniqueMusclesPerExercise_Flexibility != UserConsts.AtLeastXUniqueMusclesPerExercise_FlexibilityDefault
        || WeightIsolationXTimesMore != UserConsts.WeightIsolationXTimesMoreDefault
        || WeightSecondaryXTimesLess != UserConsts.WeightSecondaryXTimesLessDefault
        || FootnoteCountBottom != UserConsts.FootnoteCountBottomDefault
        || FootnoteCountTop != UserConsts.FootnoteCountTopDefault;

    public bool IsNewToFitness { get; init; }
    public string Token { get; init; } = null!;
    public string Email { get; init; } = null!;

    [Display(Name = "Extended Warmup", Description = "Includes joint mobilization exercises.")]
    public bool ExtendedWarmup { get; set; }

    [Display(Name = "Ignore Prerequisites", Description = "Stop checking for prerequisite exercises.")]
    public bool IgnorePrerequisites { get; set; }

    [Display(Name = "Number of User Footnotes", Description = "User footnotes appear above each workout.")]
    public int FootnoteCountTop { get; set; }

    [Display(Name = "Number of System Footnotes", Description = "System footnotes appear below each workout.")]
    public int FootnoteCountBottom { get; set; }

    [Display(Name = "Min Muscles per Mobility Exercise", Description = "A higher value will yield shorter warmup sections and a reduction in exercise variety.")]
    [Range(UserConsts.AtLeastXUniqueMusclesPerExercise_MobilityMin, UserConsts.AtLeastXUniqueMusclesPerExercise_MobilityMax)]
    public int AtLeastXUniqueMusclesPerExercise_Mobility { get; set; }

    [Display(Name = "Min Muscles per Accessory Exercise", Description = "A higher value will yield shorter accessory sections and a reduction in exercise variety.")]
    [Range(UserConsts.AtLeastXUniqueMusclesPerExercise_AccessoryMin, UserConsts.AtLeastXUniqueMusclesPerExercise_AccessoryMax)]
    public int AtLeastXUniqueMusclesPerExercise_Accessory { get; set; }

    [Display(Name = "Min Muscles per Flexibility Exercise", Description = "A higher value will yield shorter cooldown sections and a reduction in exercise variety.")]
    [Range(UserConsts.AtLeastXUniqueMusclesPerExercise_FlexibilityMin, UserConsts.AtLeastXUniqueMusclesPerExercise_FlexibilityMax)]
    public int AtLeastXUniqueMusclesPerExercise_Flexibility { get; set; }

    [Display(Name = "Weight (-) of Secondary Muscles", Description = "Changes how secondary muscles are weighted in the weekly muscle targets graph.")]
    [Range(UserConsts.WeightSecondaryXTimesLessMin, UserConsts.WeightSecondaryXTimesLessMax)]
    public double WeightSecondaryXTimesLess { get; set; }

    [Display(Name = "Weight (+) of Isolation Exercises", Description = "Changes how isolation exercises are weighted in the weekly muscle targets graph.")]
    [Range(UserConsts.WeightIsolationXTimesMoreMin, UserConsts.WeightIsolationXTimesMoreMax)]
    public double WeightIsolationXTimesMore { get; set; }
}
