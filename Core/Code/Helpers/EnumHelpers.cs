
namespace Core.Code.Helpers;

public static class EnumHelpers
{
    /// <summary>
    /// Checks if all enum values are equal to each other.
    /// Returns false if there are no values in the list.
    /// </summary>
    public static bool AllEqual<T>(params T[] values) where T : Enum
    {
        if (!values.Any())
        {
            return false;
        }

        return values.All(v => EqualityComparer<T>.Default.Equals(values[0], v));
    }
}
