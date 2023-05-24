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
}
