using Core.Dtos.User;
using Core.Models.Newsletter;

namespace Core.Dtos.Newsletter;

public class NewsletterDto
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 2;

    public DateOnly Today { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public required UserNewsletterDto User { get; init; } = null!;
    public required UserWorkoutDto UserWorkout { get; init; } = null!;

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public required Verbosity Verbosity { get; init; }

    public IList<ExerciseVariationDto> MainExercises { get; set; } = [];
    public IList<ExerciseVariationDto> PrehabExercises { get; set; } = [];
    public IList<ExerciseVariationDto> RehabExercises { get; set; } = [];
    public IList<ExerciseVariationDto> WarmupExercises { get; set; } = [];
    public IList<ExerciseVariationDto> SportsExercises { get; set; } = [];
    public IList<ExerciseVariationDto> CooldownExercises { get; set; } = [];

    /// <summary>
    /// Hiding the footer in the demo iframe.
    /// </summary>
    public bool HideFooter { get; set; } = false;

    public Client Client { get; set; } = Client.Email;
}
