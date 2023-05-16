namespace Web.Data.Query;

/// <summary>
/// How query results should be ordered.
/// </summary>
public enum OrderBy
{
    None,

    /// <summary>
    ///     Orders variations by their min progression ASC and then max progression ASC
    /// </summary>
    Progression,

    /// <summary>
    ///     Orders exercises by how many muscles they target of MuscleGroups.
    /// </summary>
    MuscleTarget,

    /// <summary>
    ///     Orders exercises by how many muscles they target of MuscleGroups.
    /// </summary>
    CoreLast,

    /// <summary>
    ///     Orders variations by their exercise name ASC and then their variation name ASC.
    /// </summary>
    Name
}