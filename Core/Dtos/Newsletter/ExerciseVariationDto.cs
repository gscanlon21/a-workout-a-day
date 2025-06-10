using Core.Dtos.Exercise;
using Core.Dtos.User;
using Core.Models.Newsletter;
using System.Diagnostics;

namespace Core.Dtos.Newsletter;

[DebuggerDisplay("{Section,nq}: {Variation,nq}")]
public class ExerciseVariationDto
{
    public Section Section { get; init; }

    public ExerciseDto Exercise { get; init; } = null!;

    public VariationDto Variation { get; init; } = null!;

    public UserExerciseDto? UserExercise { get; init; }

    public UserVariationDto? UserVariation { get; init; }

    /// <summary>
    /// Is this the user's first time viewing this exercise variation?
    /// This does not show up if the user's Intensity is set to None.
    /// </summary>
    public bool UserFirstTimeViewing { get; init; }

    public string? EasierVariation { get; init; }
    public string? HarderVariation { get; init; }

    public string? EasierReason { get; init; }
    public string? HarderReason { get; init; }

    public ProficiencyDto? Proficiency { get; init; }

    public IList<ExercisePrerequisiteDto> ExercisePrerequisites { get; init; } = [];
    public IList<ExercisePrerequisiteDto> ExercisePostrequisites { get; init; } = [];

    public override int GetHashCode() => HashCode.Combine(Exercise, Variation);
    public override bool Equals(object? obj) => obj is ExerciseVariationDto other
        && other.Exercise == Exercise && other.Variation == Variation;
}
