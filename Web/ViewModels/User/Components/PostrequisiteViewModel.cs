using Core.Models.Newsletter;
using Data.Entities.Exercise;
using Lib.ViewModels.User;
using Lib.ViewModels.Newsletter;

namespace Web.ViewModels.User.Components;

public class PostrequisiteViewModel
{
    public Verbosity Verbosity => Verbosity.Instructions | Verbosity.Images;
    public required UserNewsletterViewModel UserNewsletter { get; init; }
    public required IList<ExercisePrerequisite> Postrequisites { get; init; }
    public required IList<ExerciseVariationViewModel> VisiblePostrequisites { get; init; }
    public required IList<ExerciseVariationViewModel> InvisiblePostrequisites { get; init; }
}
