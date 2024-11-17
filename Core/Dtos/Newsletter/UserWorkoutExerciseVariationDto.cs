using Core.Dtos.Exercise;
using Core.Models.Newsletter;
using System.Text.Json.Serialization;

namespace Core.Dtos.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
public class UserWorkoutVariation
{
    public int Id { get; init; }

    public int UserWorkoutId { get; init; }

    public int VariationId { get; init; }

    /// <summary>
    /// The order of each exercise in each section.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// What section of the newsletter is this?
    /// </summary>
    public Section Section { get; init; }

    [JsonIgnore]
    public virtual UserWorkoutDto UserWorkout { get; init; } = null!;

    [JsonIgnore]
    public virtual VariationDto Variation { get; init; } = null!;
}
