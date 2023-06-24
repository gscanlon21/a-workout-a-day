using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Exercise;
using Core.Models.User;
using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.Exercise;

/// <summary>
/// Intensity level of an exercise variation
/// </summary>
public interface IExerciseVariation
{
    public int Id { get; init; }

    /// <summary>
    /// The progression range required to view the exercise variation
    /// </summary>
    [Required]
    public Progression Progression { get; init; }

    /// <summary>
    /// Where in the newsletter should this exercise be shown.
    /// </summary>
    [Required]
    public ExerciseType ExerciseType { get; init; }

    /// <summary>
    /// What sports does performing this exercise benefit.
    /// </summary>
    [Required]
    public SportsFocus SportsFocus { get; init; }

    public string? DisabledReason { get; init; }

    /// <summary>
    /// Notes about the variation (externally shown)
    /// </summary>
    public string? Notes { get; init; }

    public int ExerciseId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.Exercise.Exercise.ExerciseVariations))]
    public IExercise Exercise { get; init; }

    public int VariationId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.Exercise.Variation.ExerciseVariations))]
    public IVariation Variation { get; init; }

    //[JsonIgnore, InverseProperty(nameof(UserExerciseVariation.ExerciseVariation))]
    public ICollection<IUserExerciseVariation> UserExerciseVariations { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Newsletter.NewsletterExerciseVariation.ExerciseVariation))]
    public ICollection<INewsletterExerciseVariation> NewsletterExerciseVariations { get; init; }
}

/// <summary>
/// The range of progressions an exercise is available for.
/// </summary>
public record Progression([Range(0, 95)] int? Min, [Range(5, 100)] int? Max)
{
    public int MinOrDefault => Min ?? 0;
    public int MaxOrDefault => Max ?? 100;
}
