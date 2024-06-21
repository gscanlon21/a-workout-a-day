using Core.Dtos.Newsletter;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Core.Dtos.User;

[Table("user_frequency")]
public class UserFrequencyDto
{
    /// <summary>
    /// This is set to the Rotation Id.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    public int UserId { get; init; }

    [JsonIgnore]
    public virtual UserDto User { get; init; } = null!;

    public WorkoutRotationDto Rotation { get; set; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id, UserId);

    public override bool Equals(object? obj) => obj is UserFrequencyDto other
        && other.UserId == UserId
        && other.Id == Id;
}
