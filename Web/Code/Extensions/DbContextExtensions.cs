using Microsoft.EntityFrameworkCore;

namespace Web.Code.Extensions;

public static class DbContextExtensions
{
    public static void TryUpdateManyToMany<T, TKey>(this DbContext db, IEnumerable<T> currentItems, IEnumerable<T> newItems, Func<T, TKey> getKey) where T : class
    {
        db.Set<T>().RemoveRange(currentItems.Except(newItems, getKey));
        db.Set<T>().AddRange(newItems.Except(currentItems, getKey));
    }

    public static void AddMissing<T, TKey>(this DbContext db, IEnumerable<TKey> currentItems, IEnumerable<TKey> exercises, Func<TKey, T> newItem) where T : class
    {
        db.Set<T>().AddRange(exercises.ExceptBy(currentItems, a => a, b => b).Select(newItem));
    }
}
