﻿namespace Web.Code.Extensions;

public static class LinqExtensions
{
    /// <summary>
    /// Excepts two lists using the specified key selector.
    /// </summary>
    public static IEnumerable<T> Except<T, TKey>(this IEnumerable<T> items, IEnumerable<T> other, Func<T, TKey> getKeyFunc)
    {
        return items
            .GroupJoin(other, getKeyFunc, getKeyFunc, (item, tempItems) => new { item, tempItems })
            .SelectMany(t => t.tempItems.DefaultIfEmpty(), (t, temp) => new { t, temp })
            .Where(t => t.temp is null || t.temp.Equals(default(T)))
            .Select(t => t.t.item);
    }

    /// <summary>
    /// Excepts two lists using the specified key selector.
    /// </summary>
    public static IEnumerable<T> ExceptBy<T, Y, TKey>(this IEnumerable<T> items, IEnumerable<Y> other, Func<T, TKey> getKeyFunc, Func<Y, TKey> getYKeyFunc)
    {
        return items
            .GroupJoin(other, getKeyFunc, getYKeyFunc, (item, tempItems) => new { item, tempItems })
            .SelectMany(t => t.tempItems.DefaultIfEmpty(), (t, temp) => new { t, temp })
            .Where(t => t.temp is null || t.temp.Equals(default(T)))
            .Select(t => t.t.item);
    }
}
