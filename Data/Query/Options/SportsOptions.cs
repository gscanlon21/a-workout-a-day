using Core.Models.User;

namespace Data.Query.Options;

public class SportsOptions : IOptions
{
    public SportsOptions() { }

    public SportsOptions(SportsFocus sportsFocus)
    {
        SportsFocus = sportsFocus;
    }

    public SportsFocus? SportsFocus { get; set; }

    public bool HasData() => true;
}
