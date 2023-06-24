using Core.Models.User;
using System.ComponentModel.DataAnnotations;


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
    public NewsletterRotationViewModel NewsletterRotation { get; set; } = null!;

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

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.Newsletters))]
    public virtual User.UserViewModel User { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(NewsletterExerciseVariation.Newsletter))]
    public virtual ICollection<NewsletterExerciseVariation> NewsletterExerciseVariations { get; init; } = null!;
}
