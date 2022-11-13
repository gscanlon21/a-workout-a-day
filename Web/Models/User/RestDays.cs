namespace Web.Models.User;

/// <summary>
/// Enum of days of the week.
/// </summary>
[Flags]
public enum RestDays
{
    None = 0,
    Monday = 1 << 0,
    Tuesday = 1 << 1,
    Wednesday = 1 << 2,
    Thursday = 1 << 3,
    Friday = 1 << 4,
    Saturday = 1 << 5,
    Sunday = 1 << 6,

    All = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday,
}

public static class RestDaysExtensions
{
    /// <summary>
    /// Maps the date's day of the week to the RestDays enum.
    /// </summary>
    public static RestDays FromDate(DateOnly date)
    {
        return Enum.GetValues<RestDays>().First(r => r.ToString() == date.DayOfWeek.ToString()); 
    }
}
