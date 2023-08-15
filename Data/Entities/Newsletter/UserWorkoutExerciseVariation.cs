using Core.Models.Newsletter;
using Data.Entities.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
[Table("user_workout_exercise_variation"), Comment("A day's workout routine")]
public class UserWorkoutExerciseVariation
{
    public UserWorkoutExerciseVariation() { }

    public UserWorkoutExerciseVariation(UserWorkout newsletter, ExerciseVariation variation)
    {
        UserWorkoutId = newsletter.Id;
        ExerciseVariationId = variation.Id;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    public int UserWorkoutId { get; private init; }

    public int ExerciseVariationId { get; private init; }

    /// <summary>
    /// The order of each exercise in each section.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// What section of the newsletter is this?
    /// </summary>
    public Section Section { get; init; }

    [JsonIgnore, InverseProperty(nameof(Newsletter.UserWorkout.UserWorkoutExerciseVariations))]
    public virtual UserWorkout UserWorkout { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Exercise.ExerciseVariation.UserWorkoutExerciseVariations))]
    public virtual ExerciseVariation ExerciseVariation { get; private init; } = null!;
}
