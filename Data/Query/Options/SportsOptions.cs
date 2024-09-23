using Core.Models.Exercise;

namespace Data.Query.Options;

public class SportsOptions : IOptions
{
    public SportsOptions() { }

    public SportsOptions(SportsFocus sportsFocus)
    {
        SportsFocus = sportsFocus;
    }

    public SportsFocus? SportsFocus { get; set; }

    public bool HasData() => SportsFocus.HasValue 
        && SportsFocus != Core.Models.Exercise.SportsFocus.None;
}
