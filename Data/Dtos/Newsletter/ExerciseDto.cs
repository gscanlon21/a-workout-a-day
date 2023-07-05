using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Data.Query;
using Data.Dtos.User;
using Data.Entities.Exercise;
using Data.Entities.User;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Dtos.Newsletter;

/// <summary>
/// Viewmodel for _Exercise.cshtml
/// </summary>
[DebuggerDisplay("{Variation,nq}: {Theme}, {IntensityLevel}")]
public class ExerciseDto :
    IExerciseVariationCombo
{
    public ExerciseDto(Entities.User.User? user, Exercise exercise, Variation variation, ExerciseVariation exerciseVariation,
        UserExercise? userExercise, UserExerciseVariation? userExerciseVariation, UserVariation? userVariation,
        (string? name, string? reason) easierVariation, (string? name, string? reason) harderVariation,
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
        EasierVariation = easierVariation.name;
        HarderVariation = harderVariation.name;
        HarderReason = harderVariation.reason;
        EasierReason = easierVariation.reason;

        if (user != null)
        {
            Verbosity = user.Verbosity;

            if (UserExerciseVariation == null || UserExerciseVariation.LastSeen == DateOnly.MinValue && UserExerciseVariation.RefreshAfter == null)
            {
                UserFirstTimeViewing = true;
            }
        }
        else
        {
            Verbosity = Verbosity.Debug;
        }
    }

    public ExerciseDto(Entities.User.User? user, Exercise exercise, Variation variation, ExerciseVariation exerciseVariation,
        UserExercise? userExercise, UserExerciseVariation? userExerciseVariation, UserVariation? userVariation,
        (string? name, string? reason) easierVariation, (string? name, string? reason) harderVariation,
        IntensityLevel? intensityLevel, ExerciseTheme Theme, string token)
        : this(user, exercise, variation, exerciseVariation, userExercise, userExerciseVariation, userVariation, easierVariation: easierVariation, harderVariation: harderVariation, intensityLevel, Theme)
    {
        User = user != null ? new UserNewsletterDto(user, token) : null;
    }

    public ExerciseDto(QueryResults result, ExerciseTheme theme)
        : this(result.User, result.Exercise, result.Variation, result.ExerciseVariation,
              result.UserExercise, result.UserExerciseVariation, result.UserVariation,
              easierVariation: result.EasierVariation, harderVariation: result.HarderVariation,
              intensityLevel: null, theme)
    { }

    public ExerciseDto(QueryResults result, IntensityLevel intensityLevel, ExerciseTheme theme, string token)
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

    public Exercise Exercise { get; private init; } = null!;

    public Variation Variation { get; private init; } = null!;

    public ExerciseVariation ExerciseVariation { get; private init; } = null!;

    [JsonIgnore]
    public UserNewsletterDto? User { get; private init; }

    //[JsonIgnore]
    public UserExercise? UserExercise { get; set; }

    //[JsonIgnore]
    public UserExerciseVariation? UserExerciseVariation { get; set; }

    //[JsonIgnore]
    public UserVariation? UserVariation { get; set; }

    public bool UserFirstTimeViewing { get; private init; } = false;

    public string? EasierVariation { get; init; }
    public string? HarderVariation { get; init; }

    public string? EasierReason { get; init; }
    public string? HarderReason { get; init; }

    [UIHint("Proficiency")]
    public IList<ProficiencyDto> Proficiencies => Variation.Intensities
        .Where(intensity => intensity.IntensityLevel == IntensityLevel || IntensityLevel == null)
        .OrderBy(intensity => intensity.IntensityLevel)
        .Select(intensity => new ProficiencyDto(intensity, User, UserVariation)
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

    public override bool Equals(object? obj) => obj is ExerciseDto other
        && other.ExerciseVariation == ExerciseVariation;
}
