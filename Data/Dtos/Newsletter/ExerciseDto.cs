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
public class ExerciseDto :
    IExerciseVariationCombo
{
    public ExerciseDto(Section section, Exercise exercise, Variation variation,
        UserExercise? userExercise, UserVariation? userVariation,
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

        // Is there a user?
        if (UserVariation != null)
        {
            // Is this the user's first time viewing this exercise variation?
            if (UserVariation.LastSeen == DateOnly.MinValue && UserVariation.RefreshAfter == null)
            {
                UserFirstTimeViewing = true;
            }
        }
    }

    public ExerciseDto(QueryResults result)
        : this(result.Section, result.Exercise, result.Variation,
              result.UserExercise, result.UserVariation,
              easierVariation: result.EasierVariation, harderVariation: result.HarderVariation)
    { }

    public ExerciseDto(QueryResults result, Intensity intensity, bool needsDeload)
        : this(result.Section, result.Exercise, result.Variation,
              result.UserExercise, result.UserVariation,
              easierVariation: result.EasierVariation, harderVariation: result.HarderVariation)
    {
        Proficiency = Variation.GetProficiency(Section, intensity, needsDeload);
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

    public override int GetHashCode() => HashCode.Combine(Exercise, Variation);

    public override bool Equals(object? obj) => obj is ExerciseDto other
        && other.Exercise == Exercise && other.Variation == Variation;
}
