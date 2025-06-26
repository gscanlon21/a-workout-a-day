using System.Diagnostics;

namespace Core.Dtos.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[DebuggerDisplay("User: {UserId}, Exercise: {ExerciseId}")]
public class UserExerciseDto
{
    public int UserId { get; init; }

    public int ExerciseId { get; init; }

    public bool Ignore { get; init; }

    /// <summary>
    /// How far the user has progressed for this exercise.
    /// </summary>
    public int Progression { get; init; }

    public DateOnly LastVisible { get; init; }

    public override int GetHashCode() => HashCode.Combine(UserId, ExerciseId);
    public override bool Equals(object? obj) => obj is UserExerciseDto other
        && other.ExerciseId == ExerciseId
        && other.UserId == UserId;
}
