using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection;
using Web.Models.Exercise;

namespace Web.Extensions;

public static class EnumExtensions
{
    /// <summary> 
    /// Returns enum values where the value has a display attribute.
    /// </summary>
    public static T[] GetDisplayValues<T>() where T : struct, Enum
    {
        var props = typeof(T).GetFields();
        return Enum.GetValues<T>()
            .Where(e => props.First(f => f.Name == e.ToString()).GetCustomAttribute<DisplayAttribute>() != null)
            .ToArray();
    }

    /// <summary>
    /// Helper to check whether a [Flags] enum has any flag in the set.
    /// </summary>
    public static bool HasAnyFlag32<T>(this T flags, T oneOf) where T : Enum
    {
        return (Convert.ToInt32(flags) & Convert.ToInt32(oneOf)) != 0;
    }

    /// <summary>
    /// Helper to unset a flag from a [Flags] enum.
    /// </summary>
    public static T UnsetFlag32<T>(this T flags, T unset) where T : Enum
    {
        return (T)(object)(Convert.ToInt32(flags) & ~Convert.ToInt32(unset));
    }

    /// <summary>
    /// Returns enum values where the value has 1 or less bits set
    /// </summary>
    public static T[] GetSingleOrNoneValues32<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .Where(e => BitOperations.PopCount((ulong)Convert.ToInt32(e)) <= 1)
            .ToArray();
    }

    /// <summary>
    /// Returns enum values where the value has only 1 bit set
    /// </summary>
    public static T[] GetSingleValues32<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .Where(e => BitOperations.PopCount((ulong)Convert.ToInt32(e)) == 1)
            .ToArray();
    }

    /// <summary>
    /// Converts enum values to a select list for views
    /// </summary>
    public static IList<SelectListItem> AsSelectListItems32<T>(this IEnumerable<T> values) where T : struct, Enum
    {
        return values
            .Select(v => new SelectListItem()
            {
                Text = v.GetSingleDisplayName(),
                Value = Convert.ToInt32(v).ToString()
            })
            .ToList();
    }

    /// <summary>
    /// Name fields of the DisplayName attribute
    /// </summary>
    public enum DisplayNameType
    {
        Name,
        ShortName,
        GroupName,
        Description
    }

    /// <summary>
    /// Returns the values of the [DisplayName] attributes for each flag in the enum.
    /// </summary>
    public static string GetDisplayName32(this Enum flags, DisplayNameType nameType = DisplayNameType.Name)
    {
        if (flags == null)
        {
            return String.Empty;
        }

        var names = new HashSet<string>();
        foreach (var value in Enum.GetValues(flags.GetType()).Cast<Enum>().Where(e => Convert.ToInt32(e) > 0))
        {
            if (flags.HasFlag(value) && BitOperations.PopCount(Convert.ToUInt64(value)) == 1)
            {
                names.Add(GetSingleDisplayName(value, nameType));
            }
        }

        if (names.Count <= 0) {
            return String.Empty;
        }

        if (names.Count == 1) {
            return names.First();
        }

        return String.Join(", ", names);
    }

    /// <summary>
    /// Returns the value of the [DisplayName] attribute.
    /// </summary>
    public static string GetSingleDisplayName(this Enum @enum, DisplayNameType nameType = DisplayNameType.Name)
    {
        var memberInfo = @enum.GetType().GetMember(@enum.ToString());
        if (memberInfo != null && memberInfo.Length > 0)
        {
            var attrs = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                var attribute = (DisplayAttribute)attrs[0];
                return nameType switch
                {
                    DisplayNameType.Name => attribute.GetName(),
                    DisplayNameType.ShortName => attribute.GetShortName() ?? attribute.GetName(),
                    DisplayNameType.GroupName => attribute.GetGroupName() ?? attribute.GetShortName() ?? attribute.GetName(),
                    DisplayNameType.Description => attribute.GetDescription(),
                    _ => null
                } ?? @enum.ToString();
            }
        }

        return @enum.ToString();
    }
}
