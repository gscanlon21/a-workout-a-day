using Core.Models.Exercise;
using Core.Models.Newsletter;
using Lib.ViewModels.Exercise;
using Lib.ViewModels.User;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for _Exercise.cshtml
/// </summary>
[DebuggerDisplay("{Variation,nq}: {Theme}, {IntensityLevel}")]
public class ExerciseViewModel
{
    /// <summary>
    /// Is this exercise a warmup/cooldown or main exercise? Really the theme of the exercise view.
    /// </summary>
    public ExerciseTheme Theme { get; set; }

    public IntensityLevel? IntensityLevel { get; init; }

    public Exercise.ExerciseViewModel Exercise { get; init; } = null!;

    public VariationViewModel Variation { get; init; } = null!;

    public ExerciseVariationViewModel ExerciseVariation { get; init; } = null!;

    ////[JsonIgnore]
    //public User.UserNewsletterViewModel? User { get; init; }

    ////[JsonIgnore]
    public UserExerciseViewModel? UserExercise { get; set; }

    ////[JsonIgnore]
    public UserExerciseVariationViewModel? UserExerciseVariation { get; set; }

    ////[JsonIgnore]
    public UserVariationViewModel? UserVariation { get; set; }

    public bool UserFirstTimeViewing { get; init; } = false;

    public string? EasierVariation { get; init; }
    public string? HarderVariation { get; init; }

    public string? EasierReason { get; init; }
    public string? HarderReason { get; init; }

    /// <summary>
    /// Show's the 'Regress' link.
    /// 
    /// User's should still be able to regress if they are above the variation's max progression.
    /// </summary>
    public bool HasLowerProgressionVariation => UserExercise != null
                && UserExercise.Progression > UserExerciseViewModel.MinUserProgression
                && UserMinProgressionInRange;

    /// <summary>
    /// Shows the 'Progress' link.
    /// </summary>
    public bool HasHigherProgressionVariation => UserExercise != null
                && UserExercise.Progression < UserExerciseViewModel.MaxUserProgression
                && UserMaxProgressionInRange;

    /// <summary>
    /// Can be false if this exercise was choosen with a capped progression.
    /// </summary>
    public bool UserMinProgressionInRange => UserExercise != null
        && UserExercise.Progression >= ExerciseVariation.Progression.MinOrDefault;

    /// <summary>
    /// Can be false if this exercise was choosen with a capped progression.
    /// </summary>
    public bool UserMaxProgressionInRange => UserExercise != null
        && UserExercise.Progression < ExerciseVariation.Progression.MaxOrDefault;

    /// <summary>
    /// Can be false if this exercise was choosen with a capped progression.
    /// </summary>
    public bool UserProgressionInRange => UserMinProgressionInRange && UserMaxProgressionInRange;

    [UIHint("Proficiency")]
    public IList<ProficiencyViewModel> Proficiencies => Variation.Intensities
        .Where(intensity => intensity.IntensityLevel == IntensityLevel || IntensityLevel == null)
        .OrderBy(intensity => intensity.IntensityLevel)
        .Select(intensity => new ProficiencyViewModel(intensity, UserVariation)
        {
            ShowName = IntensityLevel == null,
            FirstTimeViewing = UserFirstTimeViewing
        })
        .ToList();

    /// <summary>
    /// How much detail to show of the exercise?
    /// </summary>
    public Verbosity Verbosity { get; set; } = Verbosity.Normal;

    public override int GetHashCode() => HashCode.Combine(ExerciseVariation);

    public override bool Equals(object? obj) => obj is ExerciseViewModel other
        && other.ExerciseVariation == ExerciseVariation;
}
