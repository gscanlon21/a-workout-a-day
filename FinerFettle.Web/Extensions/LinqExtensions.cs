
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FinerFettle.Web.Extensions
{
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
        /// Randomizes the order of a List<T> using the Fisher-Yates Shuffle.
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            int count = list.Count;
            while (count > 1)
            {
                int rand = ThreadSafeRandom.ThisThreadsRandom.Next(count--);
                (list[count], list[rand]) = (list[rand], list[count]);
            }
        }
    }
}
