using Core.Models.Newsletter;
using Core.Models.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
[Table("newsletter"), Comment("A day's workout routine")]
public class Newsletter
{
    /// <summary>
    /// Required for EF Core .AsSplitQuery()
    /// </summary>
    public Newsletter() { }

    public Newsletter(DateOnly date, User.User user, NewsletterRotation rotation, Frequency frequency, bool isDeloadWeek)
    {
        Date = date;
        User = user;
        Frequency = frequency;
        NewsletterRotation = rotation;
        IsDeloadWeek = isDeloadWeek;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    [Required]
    public int UserId { get; private init; }

    /// <summary>
    /// The date the newsletter was sent out on
    /// </summary>
    [Required]
    public DateOnly Date { get; private init; }

    [Required]
    public Client Client { get; init; }

    /// <summary>
    /// What day of the workout split was used?
    /// </summary>
    [Required]
    public NewsletterRotation NewsletterRotation { get; set; } = null!;

    /// <summary>
    /// What was the workout split used when this newsletter was sent?
    /// </summary>
    [Required]
    public Frequency Frequency { get; private init; }

    /// <summary>
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate
    /// </summary>
    [Required]
    public bool IsDeloadWeek { get; private init; }

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.Newsletters))]
    public virtual User.User User { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(NewsletterExerciseVariation.Newsletter))]
    public virtual ICollection<NewsletterExerciseVariation> NewsletterExerciseVariations { get; private init; } = null!;
}
