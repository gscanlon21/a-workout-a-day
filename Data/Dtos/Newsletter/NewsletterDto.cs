using Core.Models.Newsletter;
using Data.Dtos.User;

namespace Data.Dtos.Newsletter;

/// <summary>
/// Viewmodel for Newsletter.cshtml
/// </summary>
public class NewsletterDto(UserNewsletterDto user, Entities.Newsletter.UserWorkout newsletter)
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 2;

    public DateOnly Today { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public UserNewsletterDto User { get; } = user;
    public Entities.Newsletter.UserWorkout UserWorkout { get; } = newsletter;

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public Verbosity Verbosity { get; } = user.Verbosity;

    public IList<ExerciseVariationDto> MainExercises { get; set; } = new List<ExerciseVariationDto>();
    public IList<ExerciseVariationDto> PrehabExercises { get; set; } = new List<ExerciseVariationDto>();
    public IList<ExerciseVariationDto> RehabExercises { get; set; } = new List<ExerciseVariationDto>();
    public IList<ExerciseVariationDto> WarmupExercises { get; set; } = new List<ExerciseVariationDto>();
    public IList<ExerciseVariationDto> SportsExercises { get; set; } = new List<ExerciseVariationDto>();
    public IList<ExerciseVariationDto> CooldownExercises { get; set; } = new List<ExerciseVariationDto>();
}
