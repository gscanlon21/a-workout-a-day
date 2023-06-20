using Data.Entities.Newsletter;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

[Table("user_frequency")]
public class UserFrequency
{
    public const int MaxPerUser = 14;

    /// <summary>
    /// This is set to the Rotation Id.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [ForeignKey(nameof(Entities.User.User.Id))]
    public int UserId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserFrequencies))]
    public virtual User User { get; private init; } = null!;

    public NewsletterRotation Rotation { get; set; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id, UserId);

    public override bool Equals(object? obj) => obj is UserFrequency other
        && other.UserId == UserId
        && other.Id == Id;
}
