using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web.Models.User;

namespace Web.Entities.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
[Table("newsletter"), Comment("A day's workout routine")]
public class Newsletter
{
    public Newsletter() { }

    public Newsletter(DateOnly date, User.User user, NewsletterRotation rotation, bool isDeloadWeek)
    {
        Date = date;
        User = user;
        StrengtheningPreference = user.StrengtheningPreference;
        Frequency = user.Frequency;
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

    /// <summary>
    /// What day of the workout split was used?
    /// </summary>
    [Required]
    public NewsletterRotation NewsletterRotation { get; private init; } = null!;

    /// <summary>
    /// What was the workout split used when this newsletter was sent?
    /// </summary>
    [Required]
    public Frequency Frequency { get; private init; }

    /// <summary>
    /// What was the user's StrengtheningPreference when this newsletter was sent?
    /// </summary>
    [Required]
    public StrengtheningPreference StrengtheningPreference { get; private init; }

    /// <summary>
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate
    /// </summary>
    [Required]
    public bool IsDeloadWeek { get; private init; }

    [InverseProperty(nameof(Entities.User.User.Newsletters))]
    public virtual User.User User { get; private init; } = null!;

    [InverseProperty(nameof(NewsletterVariation.Newsletter))]
    public virtual ICollection<NewsletterVariation> NewsletterVariations { get; private init; } = null!;
}
