namespace Web.Data.Query;

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
    ///     Chooses exercises based on how many unique muscles the variation targets that have not already been worked.
    /// </summary>
    UniqueMuscles,

    /// <summary>
    ///     Orders variations by their exercise name ASC and then their variation name ASC.
    /// </summary>
    Name
}