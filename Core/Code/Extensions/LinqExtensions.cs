namespace Core.Code.Extensions;

public static class LinqExtensions
{
    /// <summary>
    /// Excepts two lists using the specified key selector.
    /// </summary>
    public static IEnumerable<O> Except<O, I, TKey>(this IEnumerable<O> outer, IEnumerable<I> inner, Func<O, TKey> outerKeyFunc, Func<I, TKey> innerKeyFunc)
    {
        return outer
            .GroupJoin(inner, outerKeyFunc, innerKeyFunc, (item, tempItems) => new { item, tempItems })
            .SelectMany(t => t.tempItems.DefaultIfEmpty(), (t, temp) => new { t, temp })
            .Where(t => t.temp is null || t.temp.Equals(default(I)))
            .Select(t => t.t.item);
    }
}
