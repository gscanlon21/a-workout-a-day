
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

    public static IOrderedEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> list, Func<T, TKey?> keySelector, NullOrder nullOrder)
        where TKey : struct
    {
        return nullOrder switch
        {
            NullOrder.NullsLast => list.OrderBy(keySelector, NullableComparer<TKey>.Larger),
            NullOrder.NullsFirst => list.OrderBy(keySelector, NullableComparer<TKey>.Smaller),
            _ => throw new ArgumentOutOfRangeException(nameof(nullOrder), nullOrder, null),
        };
    }

    public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> list, Func<T, TKey?> keySelector, NullOrder nullOrder)
        where TKey : struct
    {
        return nullOrder switch
        {
            NullOrder.NullsLast => list.ThenBy(keySelector, NullableComparer<TKey>.Larger),
            NullOrder.NullsFirst => list.ThenBy(keySelector, NullableComparer<TKey>.Smaller),
            _ => throw new ArgumentOutOfRangeException(nameof(nullOrder), nullOrder, null),
        };
    }

    public static IOrderedEnumerable<T> OrderByDescending<T, TKey>(this IEnumerable<T> list, Func<T, TKey?> keySelector, NullOrder nullOrder)
        where TKey : struct
    {
        return nullOrder switch
        {
            NullOrder.NullsLast => list.OrderByDescending(keySelector, NullableComparer<TKey>.Smaller),
            NullOrder.NullsFirst => list.OrderByDescending(keySelector, NullableComparer<TKey>.Larger),
            _ => throw new ArgumentOutOfRangeException(nameof(nullOrder), nullOrder, null),
        };
    }

    public static IOrderedEnumerable<T> ThenByDescending<T, TKey>(this IOrderedEnumerable<T> list, Func<T, TKey?> keySelector, NullOrder nullOrder)
        where TKey : struct
    {
        return nullOrder switch
        {
            NullOrder.NullsLast => list.ThenByDescending(keySelector, NullableComparer<TKey>.Smaller),
            NullOrder.NullsFirst => list.ThenByDescending(keySelector, NullableComparer<TKey>.Larger),
            _ => throw new ArgumentOutOfRangeException(nameof(nullOrder), nullOrder, null),
        };
    }

    public static IOrderedEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> list, Func<T, TKey> keySelector, NullOrder nullOrder)
        where TKey : class
    {
        return nullOrder switch
        {
            NullOrder.NullsLast => list.OrderBy(keySelector, ClassComparer<TKey>.Larger),
            NullOrder.NullsFirst => list.OrderBy(keySelector, ClassComparer<TKey>.Smaller),
            _ => throw new ArgumentOutOfRangeException(nameof(nullOrder), nullOrder, null),
        };
    }

    public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> list, Func<T, TKey> keySelector, NullOrder nullOrder)
        where TKey : class
    {
        return nullOrder switch
        {
            NullOrder.NullsLast => list.ThenBy(keySelector, ClassComparer<TKey>.Larger),
            NullOrder.NullsFirst => list.ThenBy(keySelector, ClassComparer<TKey>.Smaller),
            _ => throw new ArgumentOutOfRangeException(nameof(nullOrder), nullOrder, null),
        };
    }

    public static IOrderedEnumerable<T> OrderByDescending<T, TKey>(this IEnumerable<T> list, Func<T, TKey> keySelector, NullOrder nullOrder)
        where TKey : class
    {
        return nullOrder switch
        {
            NullOrder.NullsLast => list.OrderByDescending(keySelector, ClassComparer<TKey>.Smaller),
            NullOrder.NullsFirst => list.OrderByDescending(keySelector, ClassComparer<TKey>.Larger),
            _ => throw new ArgumentOutOfRangeException(nameof(nullOrder), nullOrder, null),
        };
    }

    public static IOrderedEnumerable<T> ThenByDescending<T, TKey>(this IOrderedEnumerable<T> list, Func<T, TKey> keySelector, NullOrder nullOrder)
        where TKey : class
    {
        return nullOrder switch
        {
            NullOrder.NullsLast => list.ThenByDescending(keySelector, ClassComparer<TKey>.Smaller),
            NullOrder.NullsFirst => list.ThenByDescending(keySelector, ClassComparer<TKey>.Larger),
            _ => throw new ArgumentOutOfRangeException(nameof(nullOrder), nullOrder, null),
        };
    }

    internal class ClassComparer<T> : IComparer<T> where T : class
    {
        public static ClassComparer<T> Larger => new ClassComparer<T>(true);
        public static ClassComparer<T> Smaller => new ClassComparer<T>(false);

        private readonly bool _isLarger;

        private ClassComparer(bool isLarger)
        {
            _isLarger = isLarger;
        }

        public int Compare(T x, T y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return _isLarger ? 1 : -1;
            }

            if (y == null)
            {
                return _isLarger ? -1 : 1;
            }

            return Comparer<T>.Default.Compare(x, y);
        }
    }

    internal class NullableComparer<T> : IComparer<T?> where T : struct
    {
        public static NullableComparer<T> Larger => new NullableComparer<T>(true);
        public static NullableComparer<T> Smaller => new NullableComparer<T>(false);

        private readonly bool _isLarger;

        private NullableComparer(bool isLarger)
        {
            _isLarger = isLarger;
        }

        public int Compare(T? x, T? y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return _isLarger ? 1 : -1;
            }

            if (y == null)
            {
                return _isLarger ? -1 : 1;
            }

            return Comparer<T>.Default.Compare(x.Value, y.Value);
        }
    }

    public enum NullOrder
    {
        NullsLast,
        NullsFirst,
    }
}
