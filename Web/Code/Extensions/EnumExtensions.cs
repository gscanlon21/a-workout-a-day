using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection;
using Web.Models.Exercise;
using Web.Models.User;

namespace Web.Code.Extensions;

public static class EnumExtensions
{
    public static IntensityLevel ToIntensityLevel(this StrengtheningPreference strengtheningPreference)
    {
        return strengtheningPreference switch
        {
            StrengtheningPreference.Light => IntensityLevel.Endurance,
            StrengtheningPreference.Medium => IntensityLevel.Hypertrophy,
            StrengtheningPreference.Heavy => IntensityLevel.Strength,
            _ => throw new NotImplementedException()
        };
    }


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
    /// Returns enum values where the value has a display attribute.
    /// </summary>
    public static Enum[] GetDisplayValues(Type t)
    {
        var props = t.GetFields();
        return Enum.GetValues(t)
            .Cast<Enum>()
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
    public static string GetDisplayName32(this Enum flags, DisplayNameType nameType = DisplayNameType.Name, bool includeAnyMatching = false)
    {
        if (flags == null)
        {
            return string.Empty;
        }

        var names = new HashSet<string>();
        var values = GetDisplayValues(flags.GetType());

        if (BitOperations.PopCount(Convert.ToUInt64(flags)) == 0)
        {
            var noneValue = values.FirstOrDefault(v => BitOperations.PopCount(Convert.ToUInt64(v)) == 0);
            if (noneValue != null)
            {
                return noneValue.GetSingleDisplayName(nameType);
            }

            return string.Empty;
        }

        foreach (var value in values)
        {
            bool isSingleValue = BitOperations.PopCount(Convert.ToUInt64(value)) == 1;
            bool hasNoSingleValue = !values.Any(v => value.HasFlag(v) && flags.HasFlag(v) && BitOperations.PopCount(Convert.ToUInt64(v)) == 1);
            bool hasFlag = includeAnyMatching ? flags.HasAnyFlag32(value) : flags.HasFlag(value);

            if (hasFlag
                // Is a compound value with none of its' values set, or is a single value that is set
                && (isSingleValue || hasNoSingleValue)
                // Skip the None value since flags has something set
                && BitOperations.PopCount(Convert.ToUInt64(value)) > 0)
            {
                names.Add(value.GetSingleDisplayName(nameType));
            }
        }

        return string.Join(", ", names);
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
