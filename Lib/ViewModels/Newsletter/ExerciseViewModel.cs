using Core.Consts;
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
