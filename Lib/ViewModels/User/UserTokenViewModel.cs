using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Lib.ViewModels.User;

/// <summary>
/// User's progression level of an exercise.
/// </summary>
public class UserTokenViewModel
{
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

    [JsonInclude]
    public UserViewModel User { get; init; } = null!;
}
