using Core.Models.Newsletter;
using System.Diagnostics;

namespace Core.Dtos.User;

/// <summary>
/// User's intensity stats.
/// </summary>
[DebuggerDisplay("User: {UserId}, Variation: {VariationId}")]
public class UserVariationDto
{
    public int UserId { get; init; }

    public int VariationId { get; init; }

    public string? Notes { get; init; }

    public Section Section { get; init; }

    /// <summary>
    /// How much weight the user is able to lift.
    /// </summary>
    public int Weight { get; init; }
    public bool HasWeight => Weight > 0;

    public int Sets { get; init; }
    public bool HasSets => Sets > 0;

    public int Reps { get; init; }
    public bool HasReps => Reps > 0;

    public int Secs { get; init; }
    public bool HasSecs => Secs > 0;

    public bool HasAbility => HasReps || HasSecs;

    public override int GetHashCode() => HashCode.Combine(UserId, VariationId, Section);
    public override bool Equals(object? obj) => obj is UserVariationDto other
        && other.VariationId == VariationId
        && other.UserId == UserId
        && other.Section == Section;
}
