using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Lib.Dtos.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[Table("user_exercise")]
[DebuggerDisplay("User: {UserId}, Exercise: {ExerciseId}")]
public class UserExercise
{
    /// <summary>
    /// The lowest the user's progression can go.
    /// 
    /// Also the user's starting progression when the user is new to fitness.
    /// </summary>
    [NotMapped]
    public const int MinUserProgression = 5;

    /// <summary>
    /// The highest the user's progression can go.
    /// </summary>
    [NotMapped]
    public const int MaxUserProgression = 95;

    [Required]
    public int UserId { get; init; }

    [Required]
    public int ExerciseId { get; init; }

    /// <summary>
    /// How far the user has progressed for this exercise.
    /// </summary>
    [Required, Range(MinUserProgression, MaxUserProgression)]
    public int Progression { get; set; } = MinUserProgression;

    /// <summary>
    /// Don't show this exercise or any of it's variations to the user
    /// </summary>
    [Required]
    public bool Ignore { get; set; }

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

    //[JsonIgnore, InverseProperty(nameof(Dtos.Exercise.Exercise.UserExercises))]
    public virtual Exercise.Exercise Exercise { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserExercises))]
    public virtual User User { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, ExerciseId);

    public override bool Equals(object? obj) => obj is UserExercise other
        && other.ExerciseId == ExerciseId
        && other.UserId == UserId;
}
