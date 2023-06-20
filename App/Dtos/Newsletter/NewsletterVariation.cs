using App.Dtos.Exercise;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace App.Dtos.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
[Table("newsletter_exercise_variation")]
public class NewsletterExerciseVariation
{
    public NewsletterExerciseVariation() { }

    public NewsletterExerciseVariation(Newsletter newsletter, ExerciseVariation variation)
    {
        NewsletterId = newsletter.Id;
        ExerciseVariationId = variation.Id;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int NewsletterId { get; init; }

    public int ExerciseVariationId { get; init; }

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

    [JsonIgnore, InverseProperty(nameof(Dtos.Newsletter.Newsletter.NewsletterExerciseVariations))]
    public virtual Newsletter Newsletter { get; init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Exercise.ExerciseVariation.NewsletterExerciseVariations))]
    public virtual ExerciseVariation ExerciseVariation { get; init; } = null!;
}
