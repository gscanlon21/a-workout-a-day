namespace Data.Query.Options;

public class SelectionOptions : IOptions
{
    public SelectionOptions() { }

    public bool IncludePrerequisites { get; set; } = true;
    public bool IncludeInstructions { get; set; } = true;
    public bool UniqueExercises { get; set; } = false;
    public bool AllRefreshed { get; set; } = false;

    public bool HasData() => true;
}
