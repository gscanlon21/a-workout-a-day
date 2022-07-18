namespace FinerFettle.Web.Extensions
{
    public static class EnumExtensions
    {
        public static IEnumerable<T> GetFlags<T>(this T flags) where T : Enum
        {
            return Enum.GetValues(flags.GetType()).Cast<T>().Where(e => flags.HasFlag(e));
        }
    }
}
