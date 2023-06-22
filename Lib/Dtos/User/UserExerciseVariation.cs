using Lib.Dtos.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Lib.Dtos.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[Table("user_exercise_variation")]
[DebuggerDisplay("User: {UserId}, ExerciseVariation: {ExerciseVariationId}")]

public class UserExerciseVariation
{
    [Required]
    public int UserId { get; init; }

    [Required]
    public int ExerciseVariationId { get; init; }

    /// <summary>
    /// When was this exercise last seen in the user's newsletter.
    /// </summary>
    [Required]
    public DateOnly LastSeen { get; set; }

    /// <summary>
    /// If this is set, will not update the LastSeen date until this date is reached.
    /// This is so we can reduce the variation of workouts and show the same groups of exercises for a month+ straight.
    /// </summary>
    public DateOnly? RefreshAfter { get; set; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserExerciseVariations))]
    public virtual User User { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(Exercise.ExerciseVariation.UserExerciseVariations))]
    public virtual ExerciseVariation ExerciseVariation { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, ExerciseVariationId);

    public override bool Equals(object? obj) => obj is UserExerciseVariation other
        && other.ExerciseVariationId == ExerciseVariationId
        && other.UserId == UserId;
}
