using Core.Models.Newsletter;
using Data.Dtos.User;

namespace Data.Dtos.Newsletter;

/// <summary>
/// Viewmodel for Newsletter.cshtml
/// </summary>
public class NewsletterDto
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 2;

    public NewsletterDto(UserNewsletterDto user, Entities.Newsletter.UserWorkout newsletter)
    {
        User = user;
        UserWorkout = newsletter;
        Verbosity = user.Verbosity;
    }

    public DateOnly Today { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public UserNewsletterDto User { get; }
    public Entities.Newsletter.UserWorkout UserWorkout { get; }

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public Verbosity Verbosity { get; }

    public required IList<ExerciseDto> MainExercises { get; init; } = null!;
    public required IList<ExerciseDto> PrehabExercises { get; init; } = null!;
    public required IList<ExerciseDto> RehabExercises { get; init; } = null!;
    public required IList<ExerciseDto> WarmupExercises { get; init; } = null!;
    public required IList<ExerciseDto> SportsExercises { get; init; } = null!;
    public required IList<ExerciseDto> CooldownExercises { get; init; } = null!;

    /// <summary>
    /// Display which equipment the user does not have.
    /// </summary>
    public required EquipmentDto Equipment { get; init; } = null!;
}
