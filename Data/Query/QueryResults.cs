using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Dtos.Newsletter;
using Data.Entities.Exercise;
using Data.Entities.User;
using System.Diagnostics;

namespace Data.Query;

[DebuggerDisplay("{Exercise}: {Variation}")]
public class QueryResults : IExerciseVariationCombo
{
    public QueryResults(Section section, Exercise exercise, Variation variation,
        UserExercise? userExercise, UserVariation? userVariation, IList<ExercisePrerequisiteDto2> exercisePrerequisites,
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
        Proficiency = Variation.GetProficiency(Section, Intensity.Light);

        if (UserVariation != null)
        {
            // Is this the user's first time viewing this exercise variation?
            if (UserVariation.LastSeen == DateOnly.MinValue)
            {
                UserFirstTimeViewing = true;
            }
        }
    }

    /*
    public QueryResults(QueryResults result)
        : this(result.Section, result.Exercise, result.Variation,
              result.UserExercise, result.UserVariation, result.ExercisePrerequisites,
              easierVariation: result.EasierVariation, harderVariation: result.HarderVariation)
    { }

    public QueryResults(QueryResults result, Intensity intensity)
        : this(result.Section, result.Exercise, result.Variation,
              result.UserExercise, result.UserVariation, result.ExercisePrerequisites,
              easierVariation: result.EasierVariation, harderVariation: result.HarderVariation)
    {
        Proficiency = Variation.GetProficiency(Section, intensity);
    }*/


    public Section Section { get; private init; }

    public Exercise Exercise { get; private init; }

    public Variation Variation { get; private init; }

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

    public IList<ExercisePrerequisiteDto2> ExercisePrerequisites { get; init; }

    public override int GetHashCode() => HashCode.Combine(Exercise, Variation);

    public override bool Equals(object? obj) => obj is QueryResults other
        && other.Exercise == Exercise && other.Variation == Variation;
}