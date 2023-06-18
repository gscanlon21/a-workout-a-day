using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Web.Entities.Exercise;
using Web.Models.Exercise;
using Web.Models.Newsletter;

namespace Web.Entities.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
[Table("newsletter_exercise_variation"), Comment("A day's workout routine")]
public class NewsletterExerciseVariation
{
    public NewsletterExerciseVariation() { }

    public NewsletterExerciseVariation(Newsletter newsletter, ExerciseVariation variation)
    {
        NewsletterId = newsletter.Id;
        ExerciseVariationId = variation.Id;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    public int NewsletterId { get; private init; }

    public int ExerciseVariationId { get; private init; }

    /// <summary>
    /// The order of each exercise in each section.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// What section of the newsletter is this?
    /// </summary>
    public Section Section { get; init; }

    /// <summary>
    /// What intensity was the variation worked at?
    /// </summary>
    public IntensityLevel? IntensityLevel { get; init; }

    [JsonIgnore, InverseProperty(nameof(Entities.Newsletter.Newsletter.NewsletterExerciseVariations))]
    public virtual Newsletter Newsletter { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Exercise.ExerciseVariation.NewsletterExerciseVariations))]
    public virtual ExerciseVariation ExerciseVariation { get; private init; } = null!;
}
