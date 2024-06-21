using Core.Dtos.Exercise;
using Core.Models.Newsletter;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Core.Dtos.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
[Table("user_workout_variation")]
public class UserWorkoutVariation
{
    public UserWorkoutVariation() { }

    public UserWorkoutVariation(UserWorkoutDto newsletter, VariationDto variation)
    {
        UserWorkoutId = newsletter.Id;
        VariationId = variation.Id;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
