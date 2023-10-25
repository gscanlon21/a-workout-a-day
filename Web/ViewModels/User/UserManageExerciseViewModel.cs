using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Entities.User;
using Lib.ViewModels.User;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserManageExerciseViewModel
{
    [Display(Name = "Exercise", Description = "Ignore this exercise and all of its variations.")]
    public required Data.Entities.Exercise.Exercise Exercise { get; init; }

    [Display(Name = "Exercise Refreshes After", Description = "Refresh this exercise—the next workout will try and select a new exercise if available.")]
    public required UserExercise UserExercise { get; init; }

    public required Data.Entities.User.User User { get; init; }

    public required Section Section { get; init; }
    public required string Email { get; init; }
    public required string Token { get; init; }

    public Verbosity ExerciseVerbosity => Verbosity.Instructions | Verbosity.Images | Verbosity.ProgressionBar;

    public required IList<Lib.ViewModels.Newsletter.ExerciseVariationViewModel> Exercises { get; init; } = null!;

    public required int ExerciseId { get; init; }
    public required int VariationId { get; init; }
}
