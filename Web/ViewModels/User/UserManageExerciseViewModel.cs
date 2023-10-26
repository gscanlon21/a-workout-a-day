using Core.Models.Newsletter;
using Data.Entities.User;
using Lib.ViewModels.Newsletter;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserManageExerciseViewModel
{
    public required UserManageExerciseVariationViewModel.Parameters Parameters { get; init; }

    public required Data.Entities.User.User User { get; init; }

    [Display(Name = "Exercise Refreshes After", Description = "Refresh this exercise—the next workout will try and select a new exercise if available.")]
    public required UserExercise UserExercise { get; init; }

    [Display(Name = "Exercise", Description = "Ignore this exercise and all of its variations.")]
    public required Data.Entities.Exercise.Exercise Exercise { get; init; }

    public required IList<ExerciseVariationViewModel> Exercises { get; init; } = null!;

    public Verbosity ExerciseVerbosity => Verbosity.Instructions | Verbosity.Images | Verbosity.ProgressionBar;

    /// <summary>
    /// Exercises aren't managed per section, ignoring the section that is only used to manage the variation.
    /// </summary>
    public Section ExerciseSection => Section.None;
}
