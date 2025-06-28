
namespace Data.Query;

public enum OrderBy
{
    None = 0,

    /// <summary>
    /// Order by progression levels.
    /// </summary>
    ProgressionLevels = 1,

    /// <summary>
    /// Order by least expected difficulty first.
    /// </summary>
    LeastDifficultFirst = 2,

    /// <summary>
    /// Show exercises that work a muscle target we want more of first.
    /// Then by hardest expected difficulty to easiest expected difficulty.
    /// </summary>
    MusclesTargeted = 3,

    /// <summary>
    /// Core exercises last.
    /// Then by hardest expected difficulty to easiest expected difficulty.
    /// </summary>
    CoreLast = 4,

    /// <summary>
    /// Order plyometrics first. They're best done early in the workout when the user isn't fatigued.
    /// Core exercises last. Ordering exercises that don't work core muscles first.
    /// Then by hardest expected difficulty to easiest expected difficulty.
    /// </summary>
    PlyometricsFirst = 5,
}
