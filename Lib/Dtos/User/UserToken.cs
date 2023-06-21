using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Lib.Dtos.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[Table("user_token")]
public class UserToken
{
    public UserToken() { }

    /// <summary>
    /// Creates a new token for the user.
    /// </summary>
    public UserToken(int userId, string token)
    {
        UserId = userId;
        Token = token;
    }

    /// <summary>
    /// Used as a unique user identifier in email links. This valus is switched out every day to expire old links.
    /// 
    /// This is kinda like a bearer token.
    /// </summary>
    [Required]
    public string Token { get; init; } = null!;

    [Required]
    public int UserId { get; init; }

    /// <summary>
    /// The token should stop working after this date.
    /// </summary>
    [Required]
    public DateOnly Expires { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserTokens))]
    public virtual User User { get; init; } = null!;
}
