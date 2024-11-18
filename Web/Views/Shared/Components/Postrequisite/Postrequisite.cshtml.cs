using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Newsletter;

namespace Web.Views.Shared.Components.Postrequisite;

public class PostrequisiteViewModel
{
    public Verbosity Verbosity => Verbosity.Instructions;
    public required UserNewsletterDto UserNewsletter { get; init; }
    public required IDictionary<int, int> ExerciseProficiencyMap { get; init; }
    public required IList<ExerciseVariationDto> VisiblePostrequisites { get; init; }
    public required IList<ExerciseVariationDto> InvisiblePostrequisites { get; init; }
}
