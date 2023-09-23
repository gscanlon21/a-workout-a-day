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
    /// When was this exercise last seen in the user's newsletter.
    /// </summary>
    [Required]
    public DateOnly LastSeen { get; set; }

    /// <summary>
    /// If this is set, will not update the LastSeen date until this date is reached.
    /// This is so we can reduce the variation of workouts and show the same groups of exercises for a month+ straight.
    /// </summary>
    public DateOnly? RefreshAfter { get; set; }

    /// <summary>
    /// How much weight the user is able to lift.
    /// </summary>
    [Required]
    public int Weight { get; set; }

    public override int GetHashCode() => HashCode.Combine(UserId, VariationId);

    public override bool Equals(object? obj) => obj is UserVariationViewModel other
        && other.VariationId == VariationId
        && other.UserId == UserId;
}
