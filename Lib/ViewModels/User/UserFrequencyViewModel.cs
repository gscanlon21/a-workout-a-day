using Lib.ViewModels.Newsletter;

namespace Lib.ViewModels.User;

public class UserFrequencyViewModel
{
    public const int MaxPerUser = 14;

    /// <summary>
    /// This is set to the Rotation Id.
    /// </summary>
    public int Id { get; set; }

    public int UserId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserFrequencies))]
    public virtual UserViewModel User { get; init; } = null!;

    public NewsletterRotationViewModel Rotation { get; set; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id, UserId);

    public override bool Equals(object? obj) => obj is UserFrequencyViewModel other
        && other.UserId == UserId
        && other.Id == Id;
}
