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

    public IList<ExerciseDto> MainExercises { get; set; } = new List<ExerciseDto>();
    public IList<ExerciseDto> PrehabExercises { get; set; } = new List<ExerciseDto>();
    public IList<ExerciseDto> RehabExercises { get; set; } = new List<ExerciseDto>();
    public IList<ExerciseDto> WarmupExercises { get; set; } = new List<ExerciseDto>();
    public IList<ExerciseDto> SportsExercises { get; set; } = new List<ExerciseDto>();
    public IList<ExerciseDto> CooldownExercises { get; set; } = new List<ExerciseDto>();

    /// <summary>
    /// Display which equipment the user does not have.
    /// </summary>
    public required EquipmentDto Equipment { get; init; } = null!;
}
