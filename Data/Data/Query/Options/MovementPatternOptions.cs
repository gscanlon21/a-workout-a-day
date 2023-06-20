using Core.Models.Exercise;

namespace Data.Data.Query.Options;

public class MovementPatternOptions : IOptions
{
    /// <summary>
    ///     If true, chooses one variation that works each unique movement pattern.
    ///     If false, chooses all variations that work any of the movement patterns.
    /// </summary>
    public bool IsUnique { get; set; } = false;

    public MovementPatternOptions() { }

    public MovementPatternOptions(MovementPattern? movementPatterns)
    {
        MovementPatterns = movementPatterns;
    }

    /// <summary>
    /// Filters the results down to only these movement patterns.
    /// </summary>
    public MovementPattern? MovementPatterns { get; } = null;
}