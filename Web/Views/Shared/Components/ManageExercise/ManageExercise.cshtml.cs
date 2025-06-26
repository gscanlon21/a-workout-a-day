using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Newsletter;
using System.ComponentModel.DataAnnotations;
using Web.Views.User;

namespace Web.Views.Shared.Components.ManageExercise;

/// <summary>
/// For CRUD actions
/// </summary>
public class ManageExerciseViewModel
{
    public required ManageExerciseVariationViewModel.Params Parameters { get; init; }

    public required Data.Entities.User.User User { get; init; }
    public required UserNewsletterDto UserNewsletter { get; init; }


    [Display(Name = "Refreshes After", Description = "Refresh this exercise—the next workout will try and select a new exercise if available.")]
    public required Data.Entities.User.UserExercise UserExercise { get; init; }

    [Display(Name = "Exercise", Description = "Ignore this exercise and all of its variations for all sections.")]
    public required Data.Entities.Exercise.Exercise Exercise { get; init; }


    public required IList<ExerciseVariationDto> ExerciseVariations { get; init; } = null!;
    public Verbosity ExerciseVerbosity => Verbosity.Instructions | Verbosity.ProgressionBar | Verbosity.Skills;
}
