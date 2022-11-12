using FinerFettle.Web.Entities.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Entities.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
[Table("newsletter_variation"), Comment("A day's workout routine")]
public class NewsletterVariation
{
    public NewsletterVariation() { }

    public NewsletterVariation(Newsletter newsletter, Variation variation) 
    { 
        NewsletterId = newsletter.Id;
        VariationId = variation.Id;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    public int NewsletterId { get; private init; }

    public int VariationId { get; private init; }

    [InverseProperty(nameof(Entities.Newsletter.Newsletter.NewsletterVariations))]
    public virtual Newsletter Newsletter { get; private init; } = null!;

    [InverseProperty(nameof(Exercise.Variation.NewsletterVariations))]
    public virtual Variation Variation { get; private init; } = null!;
}
