
namespace Core.Code.Extensions;

public static class DateOnlyExtensions
{
    public static DateOnly StartOfPreviousWeek(this DateOnly dt, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        return dt.StartOfWeek(startOfWeek: startOfWeek).AddDays(-7);
    }

    public static DateOnly EndOfPreviousWeek(this DateOnly dt, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        return dt.StartOfWeek(startOfWeek: startOfWeek).AddDays(-1);
    }

    public static DateOnly StartOfWeek(this DateOnly dt, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff);
    }

    public static DateOnly EndOfWeek(this DateOnly dt, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        return dt.StartOfWeek(startOfWeek: startOfWeek).AddDays(6);
    }

    public static DateOnly StartOfNextWeek(this DateOnly dt, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        return dt.StartOfWeek(startOfWeek: startOfWeek).AddDays(7);
    }

    public static DateOnly EndOfNextWeek(this DateOnly dt, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        return dt.StartOfWeek(startOfWeek: startOfWeek).AddDays(13);
    }

    public static DateOnly AddWeeks(this DateOnly dt, int weeks)
    {
        return dt.AddDays(7 * weeks);
    }
}
