
namespace Core.Code.Extensions;

public static class DateOnlyExtensions
{
    public static DateOnly StartOfWeek(this DateOnly dt, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff);
    }
}
