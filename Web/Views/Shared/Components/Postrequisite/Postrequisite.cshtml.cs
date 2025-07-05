using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Newsletter;

namespace Web.Views.Shared.Components.Postrequisite;

public class PostrequisiteViewModel
{
    /// <summary>
    /// Don't need to show the progression bar because
    /// the user only needs to know where they are 
    /// in this exercise's progression track.
    /// </summary>
    public Verbosity Verbosity => Verbosity.Instructions;

    public required bool Open { get; init; }
    public required UserNewsletterDto UserNewsletter { get; init; }
    public required IDictionary<int, int> ExerciseProficiencyMap { get; init; }
    public required IList<ExerciseVariationDto> VisiblePostrequisites { get; init; }
    public required IList<ExerciseVariationDto> InvisiblePostrequisites { get; init; }
}
