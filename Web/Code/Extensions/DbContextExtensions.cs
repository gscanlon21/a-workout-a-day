using Microsoft.EntityFrameworkCore;

namespace Web.Code.Extensions;

public static class DbContextExtensions
{
    /// <summary>
    /// Removes current db data that is not in the new items, and adds all new items to the db.
    /// </summary>
    public static void TryUpdateManyToMany<T, TKey>(this DbContext db, IEnumerable<T> currentItems, IEnumerable<T> newItems, Func<T, TKey> getKey) where T : class
    {
        db.Set<T>().RemoveRange(currentItems.Except(newItems, getKey, getKey));
        db.Set<T>().AddRange(newItems.Except(currentItems, getKey, getKey));
    }

    /// <summary>
    /// Adds required many-many records into the db that are not already there.
    /// </summary>
    public static void AddMissing<T, O, TKey>(this DbContext db, IEnumerable<TKey> currentItems, IEnumerable<O> allItems, Func<O, TKey> keySelector, Func<O, T> newItem) where T : class
    {
        db.Set<T>().AddRange(allItems.Except(currentItems, keySelector, a => a).Select(newItem));
    }

    /// <summary>
    /// Adds required many-many records into the db that are not already there.
    /// </summary>
    public static void AddMissing<T, TKey>(this DbContext db, IEnumerable<TKey> currentItems, IEnumerable<TKey> allItems, Func<TKey, T> newItem) where T : class
    {
        db.Set<T>().AddRange(allItems.Except(currentItems, a => a, a => a).Select(newItem));
    }
}
