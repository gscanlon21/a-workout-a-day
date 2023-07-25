using Lib.ViewModels.Newsletter;
using System.Text.Json.Serialization;

namespace Lib.ViewModels.User;

public class UserFrequencyViewModel
{
    /// <summary>
    /// This is set to the Rotation Id.
    /// </summary>
    public int Id { get; set; }

    public int UserId { get; init; }

    [JsonInclude]
    public UserViewModel User { get; init; } = null!;

    public WorkoutRotationViewModel Rotation { get; set; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id, UserId);

    public override bool Equals(object? obj) => obj is UserFrequencyViewModel other
        && other.UserId == UserId
        && other.Id == Id;
}
