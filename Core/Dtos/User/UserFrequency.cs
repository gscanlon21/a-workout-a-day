

namespace Core.Dtos.User;

public interface IUserFrequency
{
    public const int MaxPerUser = 14;

    /// <summary>
    /// This is set to the Rotation Id.
    /// </summary>
    public int Id { get; set; }

    public int UserId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserFrequencies))]
    public IUser User { get; init; }

    //public NewsletterRotation Rotation { get; set; }
}
