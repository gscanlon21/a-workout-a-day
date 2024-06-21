using Core.Models.Newsletter;
using Data.Entities.Exercise;
using Lib.Pages.Newsletter;
using Lib.Pages.Shared.Exercise;

namespace Web.Views.Shared.Components.Prerequisite;

public class PrerequisiteViewModel
{
    public Verbosity Verbosity => Verbosity.Instructions | Verbosity.Images | Verbosity.ProgressionBar;
    public required UserNewsletterViewModel UserNewsletter { get; init; }
    public required IList<ExercisePrerequisite> Prerequisites { get; init; }
    public required IList<ExerciseVariationViewModel> VisiblePrerequisites { get; init; }
    public required IList<ExerciseVariationViewModel> InvisiblePrerequisites { get; init; }

    public class ExerciseSectionComparer : IEqualityComparer<ExerciseVariationViewModel>
    {
        public bool Equals(ExerciseVariationViewModel? a, ExerciseVariationViewModel? b)
            => EqualityComparer<ExerciseViewModel>.Default.Equals(a?.Exercise, b?.Exercise);

        public int GetHashCode(ExerciseVariationViewModel e) => e.Exercise.GetHashCode();
    }
}
