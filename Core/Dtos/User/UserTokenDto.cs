using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Core.Dtos.User;

/// <summary>
/// User security token dto.
/// </summary>
public class UserTokenDto
{
    public int Id { get; init; }

    /// <summary>
    /// Used as a unique user identifier in email links. This value is switched out every day to expire old links.
    /// 
    /// This is kind of like a bearer token.
    /// </summary>
    [Required]
    public string Token { get; init; } = null!;

    [Required]
    public int UserId { get; init; }

    /// <summary>
    /// The token should stop working after this date.
    /// </summary>
    [Required]
    public DateTime Expires { get; init; } = DateTime.UtcNow.AddDays(1);

    [JsonIgnore]
    public virtual UserDto User { get; init; } = null!;
}
