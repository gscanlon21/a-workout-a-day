using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
public interface IUserToken
{
    /// <summary>
    /// Used as a unique user identifier in email links. This valus is switched out every day to expire old links.
    /// 
    /// This is kinda like a bearer token.
    /// </summary>
    [Required]
    public string Token { get; init; }

    [Required]
    public int UserId { get; init; }

    /// <summary>
    /// The token should stop working after this date.
    /// </summary>
    [Required]
    public DateOnly Expires { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserTokens))]
    public IUser User { get; init; }
}
