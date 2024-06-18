using Core.Models.Newsletter;
using Lib.ViewModels.Newsletter;
using System.ComponentModel.DataAnnotations;
using Web.ViewModels.User;

namespace Web.ViewModels.Components.UserExercise;

/// <summary>
/// For CRUD actions
/// </summary>
public class ManageExerciseViewModel
{
    public required ManageExerciseVariationViewModel.Params Parameters { get; init; }

    public required Data.Entities.User.User User { get; init; }

    [Display(Name = "Refreshes After", Description = "Refresh this exercise—the next workout will try and select a new exercise if available.")]
    public required Data.Entities.User.UserExercise UserExercise { get; init; }

    [Display(Name = "Exercise", Description = "Ignore this exercise and all of its variations for all sections.")]
    public required Data.Entities.Exercise.Exercise Exercise { get; init; }

    public required IList<ExerciseVariationViewModel> Exercises { get; init; } = null!;

    public Verbosity ExerciseVerbosity => Verbosity.Instructions | Verbosity.Images | Verbosity.ProgressionBar | Verbosity.Skills;
}
