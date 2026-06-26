using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Code.Extensions;

public static class EnumViewExtensions
{
    public enum EnumOrdering
    {
        Value = 0,
        Text = 1,
        GroupText = 2,
        Order = 3,
    }

    /// <summary>
    /// Converts enum values to a select list for views.
    /// 
    /// The default value for the enum, 0, will always come first.
    /// </summary>
    public static IEnumerable<SelectListItem> AsSelectListItems<T>(this IEnumerable<T> values, EnumOrdering order = EnumOrdering.Value, T? defaultValue = default) where T : Enum
    {
        return values.AsListItems(order, defaultValue).Select(v => new SelectListItem()
        {
            Value = Convert.ToInt64(v).ToString(),
            Text = v.GetSingleDisplayName(),
        });
    }

    /// <summary>
    /// Converts enum values to a select list for views.
    /// 
    /// The default value for the enum, 0, will always come first.
    /// </summary>
    public static IEnumerable<T> AsListItems<T>(this IEnumerable<T> values, EnumOrdering order = EnumOrdering.Value, T? defaultValue = default) where T : Enum
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
            case EnumOrdering.GroupText:
                orderedValues = orderedValues
                    .ThenBy(v => v.GetSingleDisplayName(DisplayType.GroupName))
                    .ThenBy(v => v.GetSingleDisplayName());
                break;
            case EnumOrdering.Order:
                // Careful about nulls
                orderedValues = orderedValues
                    .ThenBy(v => v.GetSingleDisplayName(DisplayType.Order).Length)
                    .ThenBy(v => v.GetSingleDisplayName(DisplayType.Order));
                break;
        }

        return orderedValues;
    }
}
