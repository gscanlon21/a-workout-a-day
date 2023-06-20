using App.ViewModels.User;
using Core.Models.Newsletter;

namespace App.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for Newsletter.cshtml
/// </summary>
public class NewsletterViewModel
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 2;

    public DateOnly Today { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public UserNewsletterViewModel User { get; init; }
    public Dtos.Newsletter.Newsletter Newsletter { get; init; }

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public Verbosity Verbosity { get; init; }

    public required IList<ExerciseViewModel> MainExercises { get; init; } = null!;
    public required IList<ExerciseViewModel> PrehabExercises { get; init; } = null!;
    public required IList<ExerciseViewModel> RehabExercises { get; init; } = null!;
    public required IList<ExerciseViewModel> WarmupExercises { get; init; } = null!;
    public required IList<ExerciseViewModel> SportsExercises { get; init; } = null!;
    public required IList<ExerciseViewModel> CooldownExercises { get; init; } = null!;
}
