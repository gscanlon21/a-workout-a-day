namespace Data.Data.Query.Options;

public class SelectionOptions : IOptions
{
    public bool UniqueExercises { get; set; } = false;
    public bool IgnoreProgressions { get; set; } = false;
    public bool IgnorePrerequisites { get; set; } = false;
}
