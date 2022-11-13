using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Web.Entities.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[Table("user_exercise_variation"), Comment("User's progression level of an exercise variation")]
[DebuggerDisplay("User: {UserId}, ExerciseVariation: {ExerciseVariationId}")]

public class UserExerciseVariation
{
    [Required]
    public int UserId { get; init; }

    [Required]
    public int ExerciseVariationId { get; init; }

    [Required]
    public DateOnly LastSeen { get; set; }

    [InverseProperty(nameof(Entities.User.User.UserExerciseVariations))]
    public virtual User User { get; private init; } = null!;

    [InverseProperty(nameof(Exercise.ExerciseVariation.UserExerciseVariations))]
    public virtual Exercise.ExerciseVariation ExerciseVariation { get; private init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, ExerciseVariationId);

    public override bool Equals(object? obj) => obj is UserExerciseVariation other
        && other.ExerciseVariationId == ExerciseVariationId
        && other.UserId == UserId;
}
