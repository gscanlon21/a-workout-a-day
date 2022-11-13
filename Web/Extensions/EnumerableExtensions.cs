namespace Web.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<TSource>? NullIfEmpty<TSource>(this IEnumerable<TSource> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return source.Any() ? source : null;
    }
}
