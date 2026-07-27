using ADay.Core.Models.Theme;
using Core.Dtos.Exercise;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Entities.Exercises;
using Data.Entities.Users;
using System.Diagnostics;
using System.Text.Json.Serialization;
using static Data.Query.Runners.BaseQueryRunner;

namespace Data.Query;

[DebuggerDisplay("{Exercise}: {Variation}")]
public class QueryResults : IExerciseVariationCombo
{
    private Theme? _theme;

    public QueryResults(Section section, InProgressQueryResults results, Intensity intensity)
    {
        Section = section;
        Exercise = results.Exercise;
        Variation = results.Variation;
        UserExercise = results.UserExercise;
        UserVariation = results.UserVariation;
        ExerciseAlternatives = results.Alternatives;
        ExercisePrerequisites = results.Prerequisites;
        ExercisePostrequisites = results.Postrequisites;

        EasierReason = results.EasierVariation.reason;
        HarderReason = results.HarderVariation.reason;
        EasierVariation = results.EasierVariation.name;
        HarderVariation = results.HarderVariation.name;

        Proficiency = intensity != Intensity.None ? Variation.GetProficiency(Section, Intensity.Light) : null;
    }

    public Section Section { get; private init; }
    public Theme Theme
    {
        get => _theme ?? Section.AsTheme();
        set => _theme = value;
    }

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

    public IList<ExerciseAlternativeDto> ExerciseAlternatives { get; init; }
    public IList<ExercisePrerequisiteDto> ExercisePrerequisites { get; init; }
    public IList<ExercisePrerequisiteDto> ExercisePostrequisites { get; init; }

    /// <summary>
    /// Is this the user's first time viewing this exercise variation?
    /// This does not show up if the user's Intensity is set to None.
    /// </summary>
    public bool UserFirstTimeViewing => (UserVariation?.FirstSeen ?? DateHelpers.Today) == DateHelpers.Today;

    public override int GetHashCode() => HashCode.Combine(Exercise, Variation);
    public override bool Equals(object? obj) => obj is QueryResults other
        && other.Exercise == Exercise && other.Variation == Variation;
}