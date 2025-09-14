
namespace Core.Code.Extensions;

public static class DateOnlyExtensions
{
    public static DateOnly AddWeeks(this DateOnly dateOnly, int weeks)
    {
        return dateOnly.AddDays(7 * weeks);
    }

    public static DateOnly StartOfPreviousWeek(this DateOnly dateOnly, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        return dateOnly.StartOfWeek(startOfWeek: startOfWeek).AddDays(-7);
    }

    public static DateOnly EndOfPreviousWeek(this DateOnly dateOnly, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        return dateOnly.StartOfWeek(startOfWeek: startOfWeek).AddDays(-1);
    }

    public static DateOnly StartOfWeek(this DateOnly dateOnly, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        int dayOfWeekDifference = (7 + (dateOnly.DayOfWeek - startOfWeek)) % 7;
        return dateOnly.AddDays(-1 * dayOfWeekDifference);
    }

    public static DateOnly EndOfWeek(this DateOnly dateOnly, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        return dateOnly.StartOfWeek(startOfWeek: startOfWeek).AddDays(6);
    }

    public static DateOnly StartOfNextWeek(this DateOnly dateOnly, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        return dateOnly.StartOfWeek(startOfWeek: startOfWeek).AddDays(7);
    }

    public static DateOnly EndOfNextWeek(this DateOnly dateOnly, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        return dateOnly.StartOfWeek(startOfWeek: startOfWeek).AddDays(13);
    }
}
