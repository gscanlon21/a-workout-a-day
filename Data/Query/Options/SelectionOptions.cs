namespace Data.Query.Options;

public class SelectionOptions : IOptions
{
    public SelectionOptions() { }

    public bool IncludePrerequisites { get; set; } = true;
    public bool IncludeInstructions { get; set; } = true;

    /// <summary>
    /// Include all variations that are due for refresh.
    /// Skips variations that have any refresh padding.
    /// </summary>
    public bool OnlyRefreshed { get; set; } = false;

    /// <summary>
    /// Orders the variations in a random order 
    /// instead of using the last seen date.
    /// </summary>
    public bool Randomized { get; set; } = false;

    public bool HasData() => true;
}
