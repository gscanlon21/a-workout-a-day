using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Web.Data.Query;
using Web.Entities.Exercise;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.Newsletter;

namespace Web.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for _Exercise.cshtml
/// </summary>
[DebuggerDisplay("{Variation,nq}: {Theme}, {IntensityLevel}")]
public class ExerciseViewModel :
    IExerciseVariationCombo
{
    public ExerciseViewModel(Entities.User.User? user, Entities.Exercise.Exercise exercise, Variation variation, ExerciseVariation exerciseVariation,
        UserExercise? userExercise, UserExerciseVariation? userExerciseVariation, UserVariation? userVariation,
        Tuple<Variation?, string?>? easierVariation, Tuple<Variation?, string?>? harderVariation,
        IntensityLevel? intensityLevel, ExerciseTheme theme)
    {
        Exercise = exercise;
        Variation = variation;
        ExerciseVariation = exerciseVariation;
        IntensityLevel = intensityLevel;
        Theme = theme;
        UserExercise = userExercise;
        UserExerciseVariation = userExerciseVariation;
        UserVariation = userVariation;
        EasierVariation = easierVariation?.Item1?.Name;
        HarderVariation = harderVariation?.Item1?.Name;
        HarderReason = harderVariation?.Item2;
        EasierReason = easierVariation?.Item2;

        if (user != null)
        {
            Verbosity = user.EmailVerbosity;

            if (UserExerciseVariation == null || UserExerciseVariation.LastSeen == DateOnly.MinValue)
            {
                UserFirstTimeViewing = true;
            }
        }
        else
        {
            Verbosity = Verbosity.Debug;
        }
    }

    public ExerciseViewModel(Entities.User.User? user, Entities.Exercise.Exercise exercise, Variation variation, ExerciseVariation exerciseVariation,
        UserExercise? userExercise, UserExerciseVariation? userExerciseVariation, UserVariation? userVariation,
        Tuple<Variation?, string?>? easierVariation, Tuple<Variation?, string?>? harderVariation,
        IntensityLevel? intensityLevel, ExerciseTheme Theme, string token)
        : this(user, exercise, variation, exerciseVariation, userExercise, userExerciseVariation, userVariation, easierVariation: easierVariation, harderVariation: harderVariation, intensityLevel, Theme)
    {
        User = user != null ? new User.UserNewsletterViewModel(user, token) : null;
    }

    public ExerciseViewModel(QueryResults result, ExerciseTheme theme)
        : this(result.User, result.Exercise, result.Variation, result.ExerciseVariation,
              result.UserExercise, result.UserExerciseVariation, result.UserVariation,
              easierVariation: result.EasierVariation, harderVariation: result.HarderVariation,
              intensityLevel: null, theme)
    { }

    public ExerciseViewModel(QueryResults result, IntensityLevel intensityLevel, ExerciseTheme theme, string token)
        : this(result.User, result.Exercise, result.Variation, result.ExerciseVariation,
              result.UserExercise, result.UserExerciseVariation, result.UserVariation,
              easierVariation: result.EasierVariation, harderVariation: result.HarderVariation,
              intensityLevel, theme, token)
    { }

    /// <summary>
    /// Is this exercise a warmup/cooldown or main exercise? Really the theme of the exercise view.
    /// </summary>
    public ExerciseTheme Theme { get; set; }

    public IntensityLevel? IntensityLevel { get; init; }

    public Entities.Exercise.Exercise Exercise { get; private init; } = null!;

    public Variation Variation { get; private init; } = null!;

    public ExerciseVariation ExerciseVariation { get; private init; } = null!;

    public User.UserNewsletterViewModel? User { get; private init; }

    public UserExercise? UserExercise { get; set; }

    public UserExerciseVariation? UserExerciseVariation { get; set; }

    public UserVariation? UserVariation { get; set; }

    public bool UserFirstTimeViewing { get; private init; } = false;

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
                && UserExercise.Progression > UserExercise.MinUserProgression
                && UserMinProgressionInRange;

    /// <summary>
    /// Shows the 'Progress' link.
    /// </summary>
    public bool HasHigherProgressionVariation => UserExercise != null
                && UserExercise.Progression < UserExercise.MaxUserProgression
                && UserMaxProgressionInRange;

    /// <summary>
    /// Can be false if this exercise was choosen with a capped progression.
    /// </summary>
    public bool UserMinProgressionInRange => UserExercise != null
        && UserExercise.Progression >= ExerciseVariation.Progression.GetMinOrDefault;

    /// <summary>
    /// Can be false if this exercise was choosen with a capped progression.
    /// </summary>
    public bool UserMaxProgressionInRange => UserExercise != null
        && UserExercise.Progression < ExerciseVariation.Progression.GetMaxOrDefault;

    /// <summary>
    /// Can be false if this exercise was choosen with a capped progression.
    /// </summary>
    public bool UserProgressionInRange => UserMinProgressionInRange && UserMaxProgressionInRange;

    [UIHint("Proficiency")]
    public IList<ProficiencyViewModel> Proficiencies => Variation.Intensities
        .Where(intensity => intensity.IntensityLevel == IntensityLevel || IntensityLevel == null)
        .OrderBy(intensity => intensity.IntensityLevel)
        .Select(intensity => new ProficiencyViewModel(intensity, User, UserVariation, Demo)
        {
            ShowName = IntensityLevel == null,
            FirstTimeViewing = UserFirstTimeViewing
        })
        .ToList();

    /// <summary>
    /// How much detail to show of the exercise?
    /// </summary>
    public Verbosity Verbosity { get; set; } = Verbosity.Normal;

    /// <summary>
    /// Should hide detail not shown in the landing page demo?
    /// </summary>
    public bool Demo => User != null && User.Features.HasFlag(Models.User.Features.Demo);

    /// <summary>
    /// User is null when the exercise is loaded on the site, not in an email newsletter.
    /// 
    /// Emails don't support scripts.
    /// </summary>
    public bool InEmailContext => User != null;

    public override int GetHashCode() => HashCode.Combine(ExerciseVariation);

    public override bool Equals(object? obj) => obj is ExerciseViewModel other
        && other.ExerciseVariation == ExerciseVariation;
}
