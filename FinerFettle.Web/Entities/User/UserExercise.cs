using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace FinerFettle.Web.Entities.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[Table("user_exercise"), Comment("User's progression level of an exercise")]
[DebuggerDisplay("User: {UserId}, Exercise: {ExerciseId}")]
public class UserExercise
{
    [NotMapped]
    public const int MinUserProgression = 5;

    [NotMapped]
    public const int MaxUserProgression = 95;

    [Required]
    public int UserId { get; init; }

    [Required]
    public int ExerciseId { get; init; }

    [Required, Range(MinUserProgression, MaxUserProgression)]
    public int Progression { get; set; }

    /// <summary>
    /// Don't show this exercise or any of it's variations to the user
    /// </summary>
    [Required]
    public bool Ignore { get; set; }

    [Required]
    public DateOnly LastSeen { get; set; }

    [InverseProperty(nameof(Entities.Exercise.Exercise.UserExercises))]
    public virtual Exercise.Exercise Exercise { get; private init; } = null!;

    [InverseProperty(nameof(Entities.User.User.UserExercises))]
    public virtual User User { get; private init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, ExerciseId);

    public override bool Equals(object? obj) => obj is UserExercise other
        && other.ExerciseId == ExerciseId
        && other.UserId == UserId;
}
