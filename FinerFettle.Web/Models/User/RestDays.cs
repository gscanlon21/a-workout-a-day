namespace FinerFettle.Web.Models.User
{
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
        Sunday = 1 << 6
    }

    public static class RestDaysExtensions
    {
        public static RestDays FromDate(DateOnly date)
        {
            return Enum.GetValues<RestDays>().Cast<RestDays>().First(r => r.ToString() == date.DayOfWeek.ToString()); 
        }
    }
}
