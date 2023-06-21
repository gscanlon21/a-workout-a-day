using Core.Code.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Code.Extensions;

public static class EnumExtensions2
{
    public enum EnumOrdering
    {
        Value = 0,
        Text = 1 << 0 // 1
    }

    /// <summary>
    /// Converts enum values to a select list for views
    /// </summary>
    public static IList<SelectListItem> AsSelectListItems32<T>(this IEnumerable<T> values, EnumOrdering order = EnumOrdering.Value, T? defaultValue = null) where T : struct, Enum
    {
        return values
            .OrderByDescending(v => Convert.ToInt32(v) == Convert.ToInt32(defaultValue))
            .ThenBy(v => order == EnumOrdering.Value ? Convert.ToInt32(defaultValue).ToString() : v.GetSingleDisplayName())
            .Select(v => new SelectListItem()
            {
                Text = v.GetSingleDisplayName(),
                Value = Convert.ToInt32(v).ToString()
            })
            .ToList();
    }
}
