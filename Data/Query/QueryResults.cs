using Core.Dtos.Exercise;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Entities.Exercise;
using Data.Entities.User;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Query;

[DebuggerDisplay("{Exercise}: {Variation}")]
public class QueryResults : IExerciseVariationCombo
{
    public QueryResults(Section section, Exercise exercise, Variation variation,
        UserExercise? userExercise, UserVariation? userVariation,
        IList<ExercisePrerequisiteDto> exercisePrerequisites, IList<ExercisePrerequisiteDto> exercisePostrequisites,
        (string? name, string? reason) easierVariation, (string? name, string? reason) harderVariation, Intensity intensity)
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
        ExercisePostrequisites = exercisePostrequisites;
        Proficiency = intensity != Intensity.None ? Variation.GetProficiency(Section, Intensity.Light) : null;
    }

    public Section Section { get; private init; }

    public Exercise Exercise { get; private init; }

    public Variation Variation { get; private init; }

    [JsonInclude]
    public UserExercise? UserExercise { get; set; }

    [JsonInclude]
    public UserVariation? UserVariation { get; set; }

    public string? EasierVariation { get; init; }
    public string? HarderVariation { get; init; }

    public string? EasierReason { get; init; }
    public string? HarderReason { get; init; }

    public Proficiency? Proficiency { get; init; }

    public IList<ExercisePrerequisiteDto> ExercisePrerequisites { get; init; }
    public IList<ExercisePrerequisiteDto> ExercisePostrequisites { get; init; }

    /// <summary>
    /// Is this the user's first time viewing this exercise variation?
    /// </summary>
    public bool UserFirstTimeViewing => (UserVariation?.FirstSeen ?? DateHelpers.Today) == DateHelpers.Today;

    public override int GetHashCode() => HashCode.Combine(Exercise, Variation);
    public override bool Equals(object? obj) => obj is QueryResults other
        && other.Exercise == Exercise && other.Variation == Variation;
}