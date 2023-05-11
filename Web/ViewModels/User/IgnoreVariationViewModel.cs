using System.ComponentModel.DataAnnotations;
using Web.Entities.Exercise;
using Web.Entities.User;
using Web.ViewModels.Newsletter;

namespace Web.ViewModels.User;

/// <summary>
/// Viewmodel for IgnoreVariation.cshtml
/// </summary>
public class IgnoreVariationViewModel
{
    [Display(Name = "Exercise", Description = "This will ignore the exercise and all of its variations.")]
    public required Entities.Exercise.Exercise? Exercise { get; init; }

    [Display(Name = "Variation", Description = "This will ignore only the variation.")]
    public required Variation Variation { get; init; }

    [Display(Name = "Exercise Refreshes After", Description = "Temporarily remove these exercise variations from your workouts.")]
    public required UserExercise UserExercise { get; init; }

    [Display(Name = "Variation Refreshes After", Description = "Temporarily remove this variation from your workouts.")]
    public required UserExerciseVariation UserExerciseVariation { get; init; }

    public bool? WasUpdated { get; init; }

    public required string Email { get; init; }
    public required string Token { get; init; }

    public required IList<ExerciseViewModel>? Exercises { get; init; }
    public required IList<ExerciseViewModel> Variations { get; init; } = null!;
}
