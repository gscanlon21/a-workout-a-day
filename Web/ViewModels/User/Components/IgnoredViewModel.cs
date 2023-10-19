using Core.Models.Newsletter;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User.Components;

public class IgnoredViewModel
{
    [Display(Name = "Ignored Exercises")]
    public required IList<Lib.ViewModels.Newsletter.ExerciseVariationViewModel> IgnoredExercises { get; init; }

    [Display(Name = "Ignored Variations")]
    public required IList<Lib.ViewModels.Newsletter.ExerciseVariationViewModel> IgnoredVariations { get; init; }

    public Verbosity Verbosity => Verbosity.Instructions | Verbosity.Images;
}
