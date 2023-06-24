using Core.Dtos.Exercise;
using System.ComponentModel.DataAnnotations;


namespace Core.Dtos.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
public interface IUserExercise
{
    /// <summary>
    /// The lowest the user's progression can go.
    /// 
    /// Also the user's starting progression when the user is new to fitness.
    /// </summary>
    public const int MinUserProgression = 5;

    /// <summary>
    /// The highest the user's progression can go.
    /// </summary>
    public const int MaxUserProgression = 95;

    [Required]
    public int UserId { get; init; }

    [Required]
    public int ExerciseId { get; init; }

    /// <summary>
    /// How far the user has progressed for this exercise.
    /// </summary>
    [Required, Range(MinUserProgression, MaxUserProgression)]
    public int Progression { get; set; }

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
    public IExercise Exercise { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserExercises))]
    public IUser User { get; init; }
}
