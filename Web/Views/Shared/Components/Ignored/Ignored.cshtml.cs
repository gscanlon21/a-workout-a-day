using Core.Models.Newsletter;
using Lib.Pages.Newsletter;
using Lib.Pages.Shared.Exercise;
using System.ComponentModel.DataAnnotations;

namespace Web.Views.Shared.Components.Ignored;

public class IgnoredViewModel
{
    [Display(Name = "Ignored Exercises")]
    public required IList<ExerciseVariationViewModel> IgnoredExercises { get; init; }

    [Display(Name = "Ignored Variations")]
    public required IList<ExerciseVariationViewModel> IgnoredVariations { get; init; }

    public Verbosity Verbosity => Verbosity.Instructions | Verbosity.Images;

    public required UserNewsletterViewModel UserNewsletter { get; init; }
}
