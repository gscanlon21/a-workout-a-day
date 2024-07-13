using Core.Models.Newsletter;
using System.Diagnostics;

namespace Core.Dtos.User;

/// <summary>
/// User's intensity stats.
/// </summary>
[DebuggerDisplay("User: {UserId}, Variation: {VariationId}")]
public class UserVariationDto
{
    public int Id { get; init; }

    public int UserId { get; init; }

    public int VariationId { get; init; }

    public string? Notes { get; set; }

    public Section Section { get; set; }

    /// <summary>
    /// Don't show this variation to the user.
    /// </summary>
    public bool Ignore { get; set; }

    /// <summary>
    /// When was this exercise last seen in the user's newsletter.
    /// </summary>
    public DateOnly LastSeen { get; set; }

    /// <summary>
    /// If this is set, will not update the LastSeen date until this date is reached.
    /// This is so we can reduce the variation of workouts and show the same groups of exercises for a month+ straight.
    /// </summary>
    public DateOnly? RefreshAfter { get; set; }

    /// <summary>
    /// How often to refresh exercises.
    /// </summary>
    public int LagRefreshXWeeks { get; set; }

    /// <summary>
    /// How often to refresh exercises.
    /// </summary>
    public int PadRefreshXWeeks { get; set; }

    /// <summary>
    /// How much weight the user is able to lift.
    /// </summary>
    public int Weight { get; set; }
    public bool HasWeight => Weight > 0;

    public int Sets { get; set; }
    public bool HasSets => Sets > 0;

    public int Reps { get; set; }
    public bool HasReps => Reps > 0;

    public int Secs { get; set; }
    public bool HasSecs => Secs > 0;

    public bool HasAbility => HasSets && (HasReps || HasSecs);

    public override int GetHashCode() => HashCode.Combine(UserId, VariationId, Section);

    public override bool Equals(object? obj) => obj is UserVariationDto other
        && other.VariationId == VariationId
        && other.UserId == UserId
        && other.Section == Section;
}
