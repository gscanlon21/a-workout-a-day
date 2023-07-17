using Lib.ViewModels.Exercise;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Lib.ViewModels.User;

/// <summary>
/// User's intensity stats.
/// </summary>
[DebuggerDisplay("User: {UserId}, Variation: {VariationId}")]
public class UserVariationViewModel
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

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserVariations))]
    public virtual UserViewModel User { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(Exercise.Variation.UserVariations))]
    public virtual VariationViewModel Variation { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, VariationId);

    public override bool Equals(object? obj) => obj is UserVariationViewModel other
        && other.VariationId == VariationId
        && other.UserId == UserId;
}
