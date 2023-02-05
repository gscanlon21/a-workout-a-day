using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Web.Entities.User;

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

    [Required]
    public DateOnly LastSeen { get; set; }

    [Required]
    public bool Ignore { get; set; }

    /// <summary>
    /// How much weight the user is able to lift
    /// </summary>
    [Required]
    public int Pounds { get; set; }

    [InverseProperty(nameof(Entities.User.User.UserVariations))]
    public virtual User User { get; private init; } = null!;

    [InverseProperty(nameof(Exercise.Variation.UserVariations))]
    public virtual Exercise.Variation Variation { get; private init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, VariationId);

    public override bool Equals(object? obj) => obj is UserVariation other
        && other.VariationId == VariationId
        && other.UserId == UserId;
}
