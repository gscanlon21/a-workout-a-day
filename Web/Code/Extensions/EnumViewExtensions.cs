using Core.Code.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Code.Extensions;

public static class EnumViewExtensions
{
    public enum EnumOrdering
    {
        Value = 0,
        Text = 1,
    }

    /// <summary>
    /// Converts enum values to a select list for views.
    /// 
    /// The default value for the enum, 0, will always come first.
    /// </summary>
    public static IList<SelectListItem> AsSelectListItems32<T>(this IEnumerable<T> values, EnumOrdering order = EnumOrdering.Value, T? defaultValue = null) where T : struct, Enum
    {
        return values
            .OrderByDescending(v => Convert.ToInt64(v) == Convert.ToInt64(defaultValue))
            .ThenBy(v => order == EnumOrdering.Value ? Convert.ToInt64(v).ToString() : v.GetSingleDisplayName())
            .Select(v => new SelectListItem()
            {
                Text = v.GetSingleDisplayName(),
                Value = Convert.ToInt64(v).ToString()
            })
            .ToList();
    }
}
