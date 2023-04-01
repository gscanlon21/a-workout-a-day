using System.ComponentModel.DataAnnotations;
using Web.Entities.Exercise;
using Web.ViewModels.Newsletter;

namespace Web.ViewModels.User;

/// <summary>
/// Viewmodel for IgnoreVariation.cshtml
/// </summary>
public class IgnoreVariationViewModel
{
    [Display(Name = "Exercise", Description = "This will ignore the exercise and all of its variations.")]
    public Entities.Exercise.Exercise? Exercise { get; init; }

    [Display(Name = "Variation", Description = "This will just ignore the variation.")]
    public required Variation Variation { get; init; }

    public bool? WasUpdated { get; init; }

    public IList<ExerciseViewModel> ExerciseVariations { get; init; } = null!;
}
