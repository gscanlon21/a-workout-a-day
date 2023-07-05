using Data.Entities.Exercise;
using Data.Entities.User;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User;

/// <summary>
/// Viewmodel for IgnoreVariation.cshtml
/// </summary>
public class ManageExerciseVariationViewModel
{
    [Display(Name = "Exercise", Description = "This will ignore the exercise and all of its variations.")]
    public required Data.Entities.Exercise.Exercise Exercise { get; init; }

    [Display(Name = "Variation", Description = "This will ignore only the variation.")]
    public required Variation Variation { get; init; }

    [Display(Name = "Exercise Refreshes After", Description = "Refresh this exercise—the next workout will try and select a new exercise if available.")]
    public required UserExercise UserExercise { get; init; }

    [Display(Name = "Variation Refreshes After", Description = "Refresh this variation—the next workout will try and select a new variation of this exercise if available.")]
    public required UserExerciseVariation UserExerciseVariation { get; init; }

    public required UserVariation UserVariation { get; init; }

    public bool? WasUpdated { get; init; }

    public required string Email { get; init; }
    public required string Token { get; init; }

    public required IList<Lib.ViewModels.Newsletter.ExerciseViewModel> Exercises { get; init; } = null!;
    public required IList<Lib.ViewModels.Newsletter.ExerciseViewModel> Variations { get; init; } = null!;
}
