using Core.Models.Newsletter;
using Data.Entities.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
[Table("user_workout_variation")]
public class UserWorkoutVariation
{
    public UserWorkoutVariation() { }

    public UserWorkoutVariation(UserWorkout newsletter, Variation variation)
    {
        UserWorkoutId = newsletter.Id;
        VariationId = variation.Id;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    public int UserWorkoutId { get; private init; }

    public int VariationId { get; private init; }

    /// <summary>
    /// The order of each exercise in each section.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// What section of the newsletter is this?
    /// </summary>
    public Section Section { get; init; }

    [JsonIgnore, InverseProperty(nameof(Newsletter.UserWorkout.UserWorkoutVariations))]
    public virtual UserWorkout UserWorkout { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Exercise.Variation.UserWorkoutVariations))]
    public virtual Variation Variation { get; private init; } = null!;
}
