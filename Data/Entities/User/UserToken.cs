using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

/// <summary>
/// User access tokens.
/// 
/// TODO Scopes.
/// TODO Single-use tokens.
/// </summary>
[Table("user_token")]
[Index(nameof(UserId), nameof(Token))]
public class UserToken
{
    [Obsolete("Public parameterless constructor required for model binding.", error: true)]
    public UserToken() { }

    /// <summary>
    /// Creates a new token for the user.
    /// </summary>
    public UserToken(User user, string token)
    {
        // Don't set User, so that EF Core doesn't add/update User.
        UserId = user.Id;
        Token = token;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; private init; }

    /// <summary>
    /// Used as a unique user identifier in email links. This value is switched out every day to expire old links.
    /// 
    /// This is kind of like a bearer token.
    /// </summary>
    [Required]
    public string Token { get; private init; } = null!;

    [Required]
    public int UserId { get; private init; }

    /// <summary>
    /// The token should stop working after this date.
    /// </summary>
    [Required]
    public DateTime Expires { get; init; } = DateTime.UtcNow.AddDays(1);

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserTokens))]
    public virtual User User { get; private init; } = null!;
}
