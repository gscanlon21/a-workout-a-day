using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Entities.Exercise;
using Data.Entities.User;
using Data.Models;
using Data.Query;
using System.Diagnostics;

namespace Data.Dtos.Newsletter;

/// <summary>
/// Viewmodel for _Exercise.cshtml
/// </summary>
[DebuggerDisplay("{Variation,nq}: {Theme}, {Intensity}")]
public class ExerciseDto :
    IExerciseVariationCombo
{
    public ExerciseDto(Section section, Exercise exercise, Variation variation, ExerciseVariation exerciseVariation,
        UserExercise? userExercise, UserExerciseVariation? userExerciseVariation, UserVariation? userVariation,
        (string? name, string? reason) easierVariation, (string? name, string? reason) harderVariation)
    {
        Section = section;
        Exercise = exercise;
        Variation = variation;
        ExerciseVariation = exerciseVariation;
        UserExercise = userExercise;
        UserExerciseVariation = userExerciseVariation;
        UserVariation = userVariation;
        EasierVariation = easierVariation.name;
        HarderVariation = harderVariation.name;
        HarderReason = harderVariation.reason;
        EasierReason = easierVariation.reason;

        // Is there a user?
        if (UserExerciseVariation != null)
        {
            // Is this the user's first time viewing this exercise variation?
            if (UserExerciseVariation.LastSeen == DateOnly.MinValue && UserExerciseVariation.RefreshAfter == null)
            {
                UserFirstTimeViewing = true;
            }
        }
    }

    public ExerciseDto(QueryResults result)
        : this(result.Section, result.Exercise, result.Variation, result.ExerciseVariation,
              result.UserExercise, result.UserExerciseVariation, result.UserVariation,
              easierVariation: result.EasierVariation, harderVariation: result.HarderVariation)
    { }

    public ExerciseDto(QueryResults result, Intensity intensity, bool needsDeload)
        : this(result.Section, result.Exercise, result.Variation, result.ExerciseVariation,
              result.UserExercise, result.UserExerciseVariation, result.UserVariation,
              easierVariation: result.EasierVariation, harderVariation: result.HarderVariation)
    {
        Proficiency = Variation.GetProficiency(Section, intensity, needsDeload);
    }

    public Section Section { get; private init; }

    public Exercise Exercise { get; private init; } = null!;

    public Variation Variation { get; private init; } = null!;

    public ExerciseVariation ExerciseVariation { get; private init; } = null!;

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

    public Proficiency? Proficiency { get; init; }

    public override int GetHashCode() => HashCode.Combine(ExerciseVariation);

    public override bool Equals(object? obj) => obj is ExerciseDto other
        && other.ExerciseVariation == ExerciseVariation;
}
