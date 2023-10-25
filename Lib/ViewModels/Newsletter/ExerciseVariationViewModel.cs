using Core.Models.Exercise;
using Core.Models.Newsletter;
using Lib.ViewModels.Exercise;
using Lib.ViewModels.User;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for _Exercise.cshtml
/// </summary>
[DebuggerDisplay("{Exercise,nq}: {Variation,nq}")]
public class ExerciseVariationViewModel
{
    public Section Section { get; init; }

    public ExerciseViewModel Exercise { get; init; } = null!;

    public VariationViewModel Variation { get; init; } = null!;

    [JsonInclude]
    public UserExerciseViewModel? UserExercise { get; set; }

    [JsonInclude]
    public UserVariationViewModel? UserVariation { get; set; }

    public bool UserFirstTimeViewing { get; init; } = false;

    public string? EasierVariation { get; init; }
    public string? HarderVariation { get; init; }

    public string? EasierReason { get; init; }
    public string? HarderReason { get; init; }

    public ProficiencyViewModel? Proficiency { get; init; }

    [JsonInclude]
    public ICollection<ExercisePrerequisiteViewModel> ExercisePrerequisites { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(Exercise, Variation);

    public override bool Equals(object? obj) => obj is ExerciseVariationViewModel other
        && other.Exercise == Exercise && other.Variation == Variation;
}
