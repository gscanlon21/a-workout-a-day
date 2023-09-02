using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User.Components;

public class IgnoredViewModel
{
    [Display(Name = "Ignored Exercises")]
    public required IList<Lib.ViewModels.Newsletter.ExerciseViewModel> IgnoredExercises { get; init; }

    [Display(Name = "Ignored Variations")]
    public required IList<Lib.ViewModels.Newsletter.ExerciseViewModel> IgnoredVariations { get; init; }
}
