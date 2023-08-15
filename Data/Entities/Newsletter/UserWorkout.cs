using Core.Models.Exercise;
using Core.Models.User;
using Data.Models.Newsletter;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
[Table("user_workout"), Comment("A day's workout routine")]
public class UserWorkout
{
    [Obsolete("Public parameterless constructor required for EF Core .AsSplitQuery()", error: true)]
    public UserWorkout() { }

    internal UserWorkout(DateOnly date, WorkoutContext context) : this(date, context.User, context.WorkoutRotation, context.Frequency, context.NeedsDeload) { }

    public UserWorkout(DateOnly date, User.User user, WorkoutRotation rotation, Frequency frequency, bool isDeloadWeek)
    {
        Date = date;
        User = user;
        Intensity = user.Intensity;
        Frequency = frequency;
        WorkoutRotation = rotation;
        IsDeloadWeek = isDeloadWeek;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    [Required]
    public int UserId { get; private init; }

    /// <summary>
    /// The date the newsletter was sent out on
    /// </summary>
    [Required]
    public DateOnly Date { get; private init; }

    /// <summary>
    /// What day of the workout split was used?
    /// </summary>
    [Required]
    public WorkoutRotation WorkoutRotation { get; set; } = null!;

    /// <summary>
    /// What was the workout split used when this newsletter was sent?
    /// </summary>
    [Required]
    public Frequency Frequency { get; private init; }

    /// <summary>
    /// What was the workout split used when this newsletter was sent?
    /// </summary>
    [Required]
    public Intensity Intensity { get; private init; }

    /// <summary>
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate
    /// </summary>
    [Required]
    public bool IsDeloadWeek { get; private init; }

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserWorkouts))]
    public virtual User.User User { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(UserWorkoutExerciseVariation.UserWorkout))]
    public virtual ICollection<UserWorkoutExerciseVariation> UserWorkoutExerciseVariations { get; private init; } = null!;
}
