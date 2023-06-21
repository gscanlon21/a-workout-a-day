using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[Table("user_token"), Comment("Auth tokens for a user")]
[Index(nameof(UserId), nameof(Token), IsUnique = false)]
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

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    /// <summary>
    /// Used as a unique user identifier in email links. This valus is switched out every day to expire old links.
    /// 
    /// This is kinda like a bearer token.
    /// </summary>
    [Required]
    public string Token { get; private init; } = null!;

    [Required]
    public int UserId { get; private init; }

    /// <summary>
    /// The token should stop working after this date.
    /// </summary>
    [Required]
    public DateOnly Expires { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserTokens))]
    public virtual User User { get; private init; } = null!;
}
