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
        var orderedValues = values.OrderByDescending(v => Convert.ToInt64(v) == Convert.ToInt64(defaultValue));
        switch (order)
        {
            case EnumOrdering.Value:
                orderedValues = orderedValues.ThenBy(v => Convert.ToInt64(v));
                break;
            case EnumOrdering.Text:
                orderedValues = orderedValues.ThenBy(v => v.GetSingleDisplayName());
                break;
        };

        return orderedValues.Select(v => new SelectListItem()
        {
            Text = v.GetSingleDisplayName(),
            Value = Convert.ToInt64(v).ToString()
        })
        .ToList();
    }
}
