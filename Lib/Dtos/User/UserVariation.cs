using Lib.Dtos.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Lib.Dtos.User;

/// <summary>
/// User's intensity stats.
/// </summary>
[Table("user_variation")]
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
    public int Pounds { get; set; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserVariations))]
    public virtual User User { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(Exercise.Variation.UserVariations))]
    public virtual Variation Variation { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, VariationId);

    public override bool Equals(object? obj) => obj is UserVariation other
        && other.VariationId == VariationId
        && other.UserId == UserId;
}
