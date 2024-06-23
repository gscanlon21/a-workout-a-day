
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
}
