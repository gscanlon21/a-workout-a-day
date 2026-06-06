
namespace Core.Code.Extensions;

public static class ListExtensions
{
    /// <summary>
    /// Implementation of the Durstenfeld shuffle.
    /// </summary>
    public static void ShuffleInPlace<TSource>(this List<TSource> source)
    {
        for (var i = source.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1); // [0, i]
            (source[i], source[j]) = (source[j], source[i]);
        }
    }

    /// <summary>
    /// Implementation of the Durstenfeld shuffle.
    /// </summary>
    public static List<TSource> ShuffleInline<TSource>(this List<TSource> source)
    {
        var result = source.ToList();
        for (var i = result.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1); // [0, i]
            (result[i], result[j]) = (result[j], result[i]);
        }

        return result;
    }
}
