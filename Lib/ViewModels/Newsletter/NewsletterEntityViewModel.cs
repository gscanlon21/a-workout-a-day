using Core.Models.User;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
public class NewsletterEntityViewModel
{
    public int Id { get; init; }

    [Required]
    public int UserId { get; init; }

    /// <summary>
    /// The date the newsletter was sent out on
    /// </summary>
    [Required]
    public DateOnly Date { get; init; }

    /// <summary>
    /// What day of the workout split was used?
    /// </summary>
    [Required]
    public WorkoutRotationViewModel Rotation { get; set; } = null!;

    /// <summary>
    /// What was the workout split used when this newsletter was sent?
    /// </summary>
    [Required]
    public Frequency Frequency { get; init; }

    /// <summary>
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate
    /// </summary>
    [Required]
    public bool IsDeloadWeek { get; init; }

    [JsonInclude]
    public User.UserViewModel User { get; init; } = null!;

    [JsonInclude]
    public ICollection<NewsletterExerciseVariation> UserWorkoutExerciseVariations { get; init; } = null!;
}
