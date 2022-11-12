using FinerFettle.Web.Data;
using FinerFettle.Web.Entities.Exercise;
using FinerFettle.Web.Entities.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace FinerFettle.Web.ViewModels.Newsletter;

[DebuggerDisplay("{Variation,nq}: {Theme}, {IntensityLevel}")]
public class ExerciseViewModel : 
    IQueryFiltersSportsFocus, 
    IQueryFiltersExerciseType, 
    IQueryFiltersIntensityLevel,
    IQueryFiltersMuscleContractions,
    IQueryFiltersMovementPatterns,
    IQueryFiltersOnlyWeights,
    IQueryFiltersUnilateral,
    IQueryFiltersMuscleMovement,
    IQueryFiltersEquipmentIds,
    IQueryFiltersRecoveryMuscle,
    IQueryFiltersMuscleGroupMuscle,
    IQueryFiltersShowCore
{
    public ExerciseViewModel(Entities.User.User? user, Entities.Exercise.Exercise exercise, Variation variation, ExerciseVariation exerciseVariation,
        UserExercise? userExercise, UserExerciseVariation? userExerciseVariation, UserVariation? userVariation, 
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
        
        if (user != null)
        {
            Verbosity = user.EmailVerbosity;

            if (UserVariation == null)
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
        IntensityLevel? intensityLevel, ExerciseTheme Theme, string token) 
        : this(user, exercise, variation, exerciseVariation, userExercise, userExerciseVariation, userVariation, intensityLevel, Theme)
    {
        User = user != null ? new User.UserNewsletterViewModel(user, token) : null;
    }

    public ExerciseViewModel(ExerciseQueryBuilder.QueryResults result, ExerciseTheme Theme) 
        : this(result.User, result.Exercise, result.Variation, result.ExerciseVariation, 
              result.UserExercise, result.UserExerciseVariation, result.UserVariation, 
              result.IntensityLevel, Theme) { }

    public ExerciseViewModel(ExerciseQueryBuilder.QueryResults result, ExerciseTheme Theme, string token) 
        : this(result.User, result.Exercise, result.Variation, result.ExerciseVariation, 
              result.UserExercise, result.UserExerciseVariation, result.UserVariation, 
              result.IntensityLevel, Theme, token) { }

    /// <summary>
    /// Is this exercise a warmup/cooldown or main exercise? Really the theme of the exercise view.
    /// </summary>
    public ExerciseTheme Theme { get; set; }

    public IntensityLevel? IntensityLevel { get; private init; }

    public Entities.Exercise.Exercise Exercise { get; private init; } = null!;

    public Variation Variation { get; private init; } = null!;

    public ExerciseVariation ExerciseVariation { get; private init; } = null!;

    public User.UserNewsletterViewModel? User { get; private init; }

    public UserExercise? UserExercise { get; set; }

    public UserExerciseVariation? UserExerciseVariation { get; set; }

    public UserVariation? UserVariation { get; set; }

    public bool UserFirstTimeViewing { get; private init; } = false;

    public bool HasLowerProgressionVariation => UserExercise != null
                && UserExercise.Progression > UserExercise.MinUserProgression;
    public bool HasHigherProgressionVariation => UserExercise != null
                && UserExercise.Progression < UserExercise.MaxUserProgression;

    [UIHint("Proficiency")]
    public IList<ProficiencyViewModel> Proficiencies => Variation.Intensities
        .Where(intensity => intensity.IntensityLevel == IntensityLevel || IntensityLevel == null)
        .OrderBy(intensity => intensity.IntensityLevel)
        .Select(intensity => new ProficiencyViewModel(intensity) { 
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
    public bool Demo => User != null && User.Email == Entities.User.User.DemoUser;

    /// <summary>
    /// Should hide detail not shown in the landing page demo?
    /// </summary>
    public bool Debug => User != null && User.Email == Entities.User.User.DebugUser;

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
