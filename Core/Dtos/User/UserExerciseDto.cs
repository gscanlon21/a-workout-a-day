using Core.Code.Helpers;
using Core.Consts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Core.Dtos.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[Table("user_exercise")]
[DebuggerDisplay("User: {UserId}, Exercise: {ExerciseId}")]
public class UserExerciseDto
{
    [Required]
    public int UserId { get; init; }

    [Required]
    public int ExerciseId { get; init; }

    /// <summary>
    /// How far the user has progressed for this exercise.
    /// </summary>
    [Required, Range(UserConsts.MinUserProgression, UserConsts.MaxUserProgression)]
    public int Progression { get; set; } = UserConsts.MinUserProgression;

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
    /// When did this exercise last have potential to be seen in the user's newsletter.
    /// </summary>
    [Required]
    public DateOnly LastVisible { get; set; } = DateHelpers.Today;

    [JsonIgnore]
    public virtual Exercise.ExerciseDto Exercise { get; set; } = null!;

    [JsonIgnore]
    public virtual UserDto User { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, ExerciseId);

    public override bool Equals(object? obj) => obj is UserExerciseDto other
        && other.ExerciseId == ExerciseId
        && other.UserId == UserId;
}
