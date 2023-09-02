using Core.Models.Exercise;
using Core.Models.Newsletter;
using System.Text.Json.Serialization;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
public class NewsletterExerciseVariation
{
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
    public Intensity? Intensity { get; init; }

    [JsonInclude]
    public NewsletterEntityViewModel Newsletter { get; init; } = null!;
}
