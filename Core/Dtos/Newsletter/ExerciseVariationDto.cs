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

    //[JsonIgnore]
    public UserExerciseDto? UserExercise { get; set; }

    //[JsonIgnore]
    public UserVariationDto? UserVariation { get; set; }

    public bool UserFirstTimeViewing { get; init; } = false;

    public string? EasierVariation { get; init; }
    public string? HarderVariation { get; init; }

    public string? EasierReason { get; init; }
    public string? HarderReason { get; init; }

    public ProficiencyDto? Proficiency { get; init; }

    public IList<ExercisePrerequisiteDto> ExercisePrerequisites { get; init; }

    public override int GetHashCode() => HashCode.Combine(Exercise, Variation);

    public override bool Equals(object? obj) => obj is ExerciseVariationDto other
        && other.Exercise == Exercise && other.Variation == Variation;
}
