using Core.Dtos.Exercise;
using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
public interface IUserExerciseVariation
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
    public IUser User { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Exercise.ExerciseVariation.UserExerciseVariations))]
    public IExerciseVariation ExerciseVariation { get; init; }
}
