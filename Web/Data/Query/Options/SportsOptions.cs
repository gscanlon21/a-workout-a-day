using Web.Models.User;

namespace Web.Data.Query.Options;

public class SportsOptions
{
    public SportsOptions() { }

    public SportsOptions(SportsFocus sportsFocus)
    {
        SportsFocus = sportsFocus;
    }

    public SportsFocus? SportsFocus { get; set; }
}
