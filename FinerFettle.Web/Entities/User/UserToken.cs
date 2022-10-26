using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Entities.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[Table("user_token"), Comment("Auth tokens for a user")]
public class UserToken
{
    public UserToken() { }

    public UserToken(int userId)
    {
        UserId = userId;

        Token = $"{Guid.NewGuid()}";
    }

    /// <summary>
    /// Used as a unique user identifier in email links. This valus is switched out every day to expire old links.
    /// 
    /// This is kinda like a bearer token.
    /// </summary>
    [Required]
    public string Token { get; private init; } = null!;

    [Required]
    public int UserId { get; private init; }

    [Required]
    public DateOnly Expires { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

    [InverseProperty(nameof(Entities.User.User.UserTokens))]
    public virtual User User { get; private init; } = null!;
}
