using Core.Consts;
using System.ComponentModel.DataAnnotations;

using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Lib.ViewModels.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[DebuggerDisplay("User: {UserId}, Exercise: {ExerciseId}")]
public class UserExerciseViewModel
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
    /// If this is set, will not update the LastSeen date until this date is reached.
    /// This is so we can reduce the variation of workouts and show the same groups of exercises for a month+ straight.
    /// </summary>
    public DateOnly? RefreshAfter { get; set; }

    [JsonInclude]
    public Exercise.ExerciseViewModel Exercise { get; init; } = null!;

    [JsonInclude]
    public UserViewModel User { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, ExerciseId);

    public override bool Equals(object? obj) => obj is UserExerciseViewModel other
        && other.ExerciseId == ExerciseId
        && other.UserId == UserId;
}
