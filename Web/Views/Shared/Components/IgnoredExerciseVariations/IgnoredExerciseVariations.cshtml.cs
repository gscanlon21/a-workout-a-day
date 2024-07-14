using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Newsletter;
using System.ComponentModel.DataAnnotations;

namespace Web.Views.Shared.Components.IgnoredExerciseVariations;

public class IgnoredExerciseVariationsViewModel
{
    [Display(Name = "Ignored Exercises")]
    public required IList<ExerciseVariationDto> IgnoredExercises { get; init; }

    [Display(Name = "Ignored Variations")]
    public required IList<ExerciseVariationDto> IgnoredVariations { get; init; }

    public required UserNewsletterDto UserNewsletter { get; init; }

    public Verbosity Verbosity => Verbosity.Instructions;
}
