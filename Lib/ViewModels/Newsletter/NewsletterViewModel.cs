using Core.Models.Newsletter;
using Lib.ViewModels.User;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for Newsletter.cshtml
/// </summary>
public class NewsletterViewModel
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 2;

    public DateOnly Today { get; init; }

    public UserNewsletterViewModel User { get; init; } = null!;
    public NewsletterEntityViewModel UserWorkout { get; init; } = null!;

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public Verbosity Verbosity { get; init; }

    public IList<ExerciseVariationViewModel> Exercises { get; init; } = null!;

    public IList<ExerciseVariationViewModel> MainExercises { get; init; } = null!;
    public IList<ExerciseVariationViewModel> PrehabExercises { get; init; } = null!;
    public IList<ExerciseVariationViewModel> RehabExercises { get; init; } = null!;
    public IList<ExerciseVariationViewModel> WarmupExercises { get; init; } = null!;
    public IList<ExerciseVariationViewModel> SportsExercises { get; init; } = null!;
    public IList<ExerciseVariationViewModel> CooldownExercises { get; init; } = null!;

    /// <summary>
    /// Hiding the footer in the demo iframe.
    /// </summary>
    public bool HideFooter { get; set; } = false;
}
