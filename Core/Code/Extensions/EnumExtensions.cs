using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection;

namespace Core.Code.Extensions;

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
    public static T As<T>(this Enum flags, T? defaultVal = null) where T : struct, Enum
    {
        if (Enum.IsDefined((T)(object)flags))
        {
            return (T)(object)flags;
        }

        return defaultVal ?? (T)(object)0;
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
    /// Returns enum values where the value has 1 or more bits set
    /// </summary>
    public static T[] GetSingleValuesExcluding32<T>(params T[] excludes) where T : struct, Enum
    {
        var excludeValues = excludes.Select(exclude => Convert.ToInt32(exclude));
        return Enum.GetValues<T>()
            .Where(e => BitOperations.PopCount((ulong)Convert.ToInt32(e)) == 1)
            .Where(e => !excludeValues.Contains(Convert.ToInt32(e)))
            .ToArray();
    }

    /// <summary>
    /// Returns enum values where the value has 1 or more bits set
    /// </summary>
    public static T[] GetSingleValuesExcludingAny32<T>(T excludes) where T : struct, Enum
    {
        var excludeValues = Convert.ToInt32(excludes);
        return Enum.GetValues<T>()
            .Where(e => BitOperations.PopCount((ulong)Convert.ToInt32(e)) == 1)
            .Where(e => (excludeValues & Convert.ToInt32(e)) == 0)
            .ToArray();
    }

    /// <summary>
    /// Returns enum values where the value has 1 or more bits set
    /// </summary>
    public static T[] GetNotNoneValues32<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .Where(e => BitOperations.PopCount((ulong)Convert.ToInt32(e)) >= 1)
            .ToArray();
    }

    /// <summary>
    /// Returns enum values where the value has 1 or more bits set
    /// </summary>
    public static T[] GetValuesExcluding32<T>(params T[] excludes) where T : struct, Enum
    {
        var excludeValues = excludes.Select(exclude => Convert.ToInt32(exclude));
        return Enum.GetValues<T>()
            .Where(e => !excludeValues.Contains(Convert.ToInt32(e)))
            .ToArray();
    }

    /// <summary>
    /// Returns enum values where the value has 1 or more bits set
    /// </summary>
    public static T[] GetValues32<T>(params T[] includes) where T : struct, Enum
    {
        var includeValues = includes.Select(include => Convert.ToInt32(include));
        return Enum.GetValues<T>()
            .Where(e => includeValues.Contains(Convert.ToInt32(e)))
            .ToArray();
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
                } ?? @enum.GetDisplayName32(nameType);
            }
        }

        return @enum.GetDisplayName32(nameType);
    }

    /// <summary>
    /// Returns the value of the [DisplayName] attribute.
    /// </summary>
    public static string? GetSingleDisplayNameOrNull(this Enum @enum, DisplayNameType nameType = DisplayNameType.Name)
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
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the value of the [DisplayName] attribute.
    /// </summary>
    public static string GetDisplayName322<T>(this T @enum, DisplayNameType nameType = DisplayNameType.Name) where T : struct, Enum
    {
        var results = new Dictionary<int, string?>();
        foreach (var value in Enum.GetValues<T>().OrderByDescending(e => BitOperations.PopCount((ulong)Convert.ToInt32(e))))
        {
            // If enum has all the values of the value we are checking.
            if ((Convert.ToInt32(@enum) & Convert.ToInt32(value)) == Convert.ToInt32(value))
            {
                // The value does not have all the values in all of the results.
                if (!((results.Aggregate(0, (curr, n) => Convert.ToInt32(curr) | Convert.ToInt32(n.Key)) & Convert.ToInt32(value)) == Convert.ToInt32(value))
                    // The value does not have any flags set in any of the results. 
                    && !results.Any(r => (Convert.ToInt32(r.Key) & Convert.ToInt32(value)) > 0))
                {
                    results.Add(Convert.ToInt32(value), value.GetSingleDisplayNameOrNull(nameType));
                }
            }
        }

        return string.Join(", ", results.Values.Where(v => v != null));
    }
}
