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
[DebuggerDisplay("{Section,nq}: {Variation,nq}")]
public class ExerciseVariationDto :
    IExerciseVariationCombo
{
    public ExerciseVariationDto(Section section, Exercise exercise, Variation variation,
        UserExercise? userExercise, UserVariation? userVariation, IList<ExercisePrerequisiteDto> exercisePrerequisites,
        (string? name, string? reason) easierVariation, (string? name, string? reason) harderVariation)
    {
        Section = section;
        Exercise = exercise;
        Variation = variation;
        UserExercise = userExercise;
        UserVariation = userVariation;
        EasierVariation = easierVariation.name;
        HarderVariation = harderVariation.name;
        HarderReason = harderVariation.reason;
        EasierReason = easierVariation.reason;
        ExercisePrerequisites = exercisePrerequisites;

        if (UserVariation != null)
        {
            // Is this the user's first time viewing this exercise variation?
            if (UserVariation.LastSeen == DateOnly.MinValue)
            {
                UserFirstTimeViewing = true;
            }
        }
    }

    public ExerciseVariationDto(QueryResults result)
        : this(result.Section, result.Exercise, result.Variation,
              result.UserExercise, result.UserVariation, result.ExercisePrerequisites,
              easierVariation: result.EasierVariation, harderVariation: result.HarderVariation)
    { }

    public ExerciseVariationDto(QueryResults result, Intensity intensity, bool needsDeload)
        : this(result.Section, result.Exercise, result.Variation,
              result.UserExercise, result.UserVariation, result.ExercisePrerequisites,
              easierVariation: result.EasierVariation, harderVariation: result.HarderVariation)
    {
        Proficiency = Variation.GetProficiency(Section, intensity, result.Variation.ExerciseFocus, needsDeload);
    }

    public Section Section { get; private init; }

    public Exercise Exercise { get; private init; } = null!;

    public Variation Variation { get; private init; } = null!;

    //[JsonIgnore]
    public UserExercise? UserExercise { get; set; }

    //[JsonIgnore]
    public UserVariation? UserVariation { get; set; }

    public bool UserFirstTimeViewing { get; private init; } = false;

    public string? EasierVariation { get; init; }
    public string? HarderVariation { get; init; }

    public string? EasierReason { get; init; }
    public string? HarderReason { get; init; }

    public Proficiency? Proficiency { get; init; }

    public IList<ExercisePrerequisiteDto> ExercisePrerequisites { get; init; }

    public override int GetHashCode() => HashCode.Combine(Exercise, Variation);

    public override bool Equals(object? obj) => obj is ExerciseVariationDto other
        && other.Exercise == Exercise && other.Variation == Variation;
}
