
namespace Core.Code.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Returns null if the source list does not contain any items.
    /// </summary>
    public static IEnumerable<TSource>? NullIfEmpty<TSource>(this IEnumerable<TSource>? source)
    {
        return source?.Any() == true ? source : null;
    }

    /// <summary>
    /// Returns true if the source list has any elements and matches the predicate.
    /// </summary>
    public static bool AllIfAny<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        return source.Any() && source.All(predicate);
    }
}
