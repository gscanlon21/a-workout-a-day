using Core.Models.Newsletter;
using Lib.ViewModels.Newsletter;
using Lib.ViewModels.User;

namespace Web.ViewModels.User.Components;

public class PrerequisiteViewModel
{
    public Verbosity Verbosity => Verbosity.Instructions | Verbosity.Images;
    public required UserNewsletterViewModel UserNewsletter { get; init; }
    public required IList<ExerciseVariationViewModel> Prerequisites { get; init; }
}
