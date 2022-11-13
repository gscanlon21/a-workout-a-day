using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Functions.Models.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
[Table("user_token")]
public class UserToken
{
    /// <summary>
    /// Used as a unique user identifier in email links. This valus is switched out every day to expire old links.
    /// 
    /// This is kinda like a bearer token.
    /// </summary>
    public string Token { get; private init; } = null!;

    public int UserId { get; private init; }

    /// <summary>
    /// Unsubscribe links need to work for at least 60 days per law
    /// </summary>
    public DateOnly Expires { get; private init; }

    [InverseProperty(nameof(Models.User.User.UserTokens))]
    public virtual User User { get; private init; } = null!;
}
