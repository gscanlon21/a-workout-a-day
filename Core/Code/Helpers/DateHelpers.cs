
using Core.Models.User;

namespace Core.Code.Helpers;

public static class DateHelpers
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// This week's Sunday date in UTC.
    /// </summary>
    public static DateOnly StartOfWeek => Today.AddDays(-1 * (int)Today.DayOfWeek);

    /// <summary>
    /// The current hour in UTC.
    /// </summary>
    public static int CurrentHour => int.Parse(DateTime.UtcNow.ToString("HH"));

    /// <summary>
    /// What day of the week is it in UTC.
    /// </summary>
    public static Days CurrentDay => DaysExtensions.FromDate(Today);
}
