using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

/// <summary>
/// User's intensity stats.
/// </summary>
[Table("user_variation"), Comment("User's intensity stats")]
[DebuggerDisplay("User: {UserId}, Variation: {VariationId}")]

public class UserVariation
{
    [Required]
    public int UserId { get; init; }

    [Required]
    public int VariationId { get; init; }

    /// <summary>
    /// Don't show this variation to the user.
    /// </summary>
    [Required]
    public bool Ignore { get; set; }

    /// <summary>
    /// How much weight the user is able to lift.
    /// </summary>
    [Required]
    public int Weight { get; set; }

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserVariations))]
    public virtual User User { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Exercise.Variation.UserVariations))]
    public virtual Exercise.Variation Variation { get; private init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, VariationId);

    public override bool Equals(object? obj) => obj is UserVariation other
        && other.VariationId == VariationId
        && other.UserId == UserId;
}
