using Lib.ViewModels.Exercise;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Lib.ViewModels.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[DebuggerDisplay("User: {UserId}, ExerciseVariation: {ExerciseVariationId}")]
public class UserExerciseVariationViewModel
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

    [JsonInclude]
    public UserViewModel User { get; init; } = null!;

    [JsonInclude]
    public ExerciseVariationViewModel ExerciseVariation { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, ExerciseVariationId);

    public override bool Equals(object? obj) => obj is UserExerciseVariationViewModel other
        && other.ExerciseVariationId == ExerciseVariationId
        && other.UserId == UserId;
}
