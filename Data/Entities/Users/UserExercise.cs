using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Users;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[Table("user_exercise")]
[DebuggerDisplay("User: {UserId}, Exercise: {ExerciseId}")]
public class UserExercise
{
    [Required]
    public int UserId { get; init; }

    [Required]
    public int ExerciseId { get; init; }

    /// <summary>
    /// How far the user has progressed for this exercise.
    /// </summary>
    [Required, Range(UserConsts.UserProgressionMin, UserConsts.UserProgressionMax)]
    public int Progression { get; set; } = UserConsts.UserProgressionMin;

    /// <summary>
    /// Don't show this exercise or any of it's variations to the user
    /// </summary>
    [Required]
    public bool Ignore { get; set; }

    /// <summary>
    /// When was this exercise last seen in the user's newsletter.
    /// This always updates, regardless of the variation's RefreshAfter date.
    /// </summary>
    public DateOnly? LastSeen { get; set; }

    /// <summary>
    /// When was this record created?
    /// </summary>
    public DateOnly? FirstSeen { get; set; }

    /// <summary>
    /// When did this exercise last have potential to be seen in the user's newsletter.
    /// </summary>
    [Required]
    public DateOnly LastVisible { get; set; } = DateHelpers.Today;

    [JsonIgnore, InverseProperty(nameof(Entities.Exercise.Exercise.UserExercises))]
    public virtual Exercise.Exercise Exercise { get; set; } = null!;

    [JsonIgnore, InverseProperty(nameof(Entities.Users.User.UserExercises))]
    public virtual User User { get; private init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, ExerciseId);
    public override bool Equals(object? obj) => obj is UserExercise other
        && other.ExerciseId == ExerciseId
        && other.UserId == UserId;
}
