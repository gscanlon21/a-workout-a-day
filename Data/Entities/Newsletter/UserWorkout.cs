using Core.Models.Exercise;
using Core.Models.User;
using Data.Entities.Users;
using Data.Models.Newsletter;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.Newsletter;

/// <summary>
/// A day's workout routine.
/// </summary>
[Table("user_workout")]
//[Index(nameof(UserId), nameof(Date))]
public class UserWorkout
{
    [Obsolete("Public parameterless constructor required for EF Core.", error: true)]
    public UserWorkout() { }

    internal UserWorkout(WorkoutContext context) : this(context.Date, context.User, context.WorkoutRotation.AsType<WorkoutRotation>()!, context.Frequency, context.Intensity, context.NeedsDeload) { }

    public UserWorkout(DateOnly date, User user, WorkoutRotation rotation, Frequency frequency, Intensity intensity, bool isDeloadWeek)
    {
        // Don't set User, so that EF Core doesn't add/update User.
        UserId = user.Id;
        Date = date;
        Intensity = intensity;
        Frequency = frequency;
        Rotation = rotation;
        IsDeloadWeek = isDeloadWeek;
        Logs = UserLogs.WriteLogs(user);
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    [Required]
    public int UserId { get; private init; }

    /// <summary>
    /// The date the workout is for, using the user's UTC offset date.
    /// </summary>
    [Required]
    public DateOnly Date { get; private init; }

    /// <summary>
    /// What day of the workout split was used?
    /// </summary>
    [Required]
    public WorkoutRotation Rotation { get; set; } = null!;

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

    public string? Logs { get; private init; }


    #region Navigation Properties

    [JsonIgnore, InverseProperty(nameof(UserWorkoutVariation.UserWorkout))]
    public virtual ICollection<UserWorkoutVariation> UserWorkoutVariations { get; private init; } = null!;

    #endregion
}
