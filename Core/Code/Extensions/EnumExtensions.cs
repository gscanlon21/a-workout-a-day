using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection;

namespace Core.Code.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// Returns the population count (number of bits set) of a mask.
    /// </summary>
    public static int PopCount<T>(this T flags) where T : struct, Enum
    {
        try
        {
            return BitOperations.PopCount(Convert.ToUInt64(flags));
        }
        catch (OverflowException ex)
        {
            var exception = new EnumOverflowException(ex.Message, ex);
            exception.Data[nameof(T)] += typeof(T).Name;
            throw exception;
        }
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
        return (Convert.ToInt64(flags) & Convert.ToInt64(oneOf)) != 0;
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

        return defaultVal ?? default;
    }

    /// <summary>
    /// Helper to unset a flag from a [Flags] enum.
    /// </summary>
    public static T UnsetFlag32<T>(this T flags, T unset) where T : Enum
    {
        return (T)Enum.ToObject(typeof(T), Convert.ToInt64(flags) & ~Convert.ToInt64(unset));
    }

    /// <summary>
    /// Returns enum values where the value has 1 or less bits set.
    /// </summary>
    public static T[] GetSingleOrNoneValues32<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>().Where(e => e.PopCount() <= 1).ToArray();
    }

    /// <summary>
    /// Returns enum values where the value has only 1 bit set.
    /// </summary>
    public static T[] GetSingleValues32<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>().Where(e => e.PopCount() == 1).ToArray();
    }

    /// <summary>
    /// Returns enum values where the value only has 1 bit set and the value is contained in the argument value.
    /// </summary>
    public static T[] GetSubValues32<T>(T value) where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .Where(e => e.PopCount() == 1)
            .Where(e => value.HasFlag(e))
            .ToArray();
    }

    /// <summary>
    /// Returns enum values where the value has more than 1 bit set.
    /// </summary>
    public static T[] GetMultiValues32<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>().Where(e => e.PopCount() > 1).ToArray();
    }

    /// <summary>
    /// Returns enum values where the value only has 1 bit set.
    /// </summary>
    public static T[] GetSingleValuesExcludingAny32<T>(T excludes) where T : struct, Enum
    {
        var excludeValues = Convert.ToInt64(excludes);
        return Enum.GetValues<T>()
            .Where(e => e.PopCount() == 1)
            .Where(e => (excludeValues & Convert.ToInt64(e)) == 0)
            .ToArray();
    }

    /// <summary>
    /// Returns enum values where the value has 1 or more bits set.
    /// </summary>
    public static T[] GetNotNoneValues32<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>().Where(e => e.PopCount() >= 1).ToArray();
    }

    /// <summary>
    /// Returns enum values where the value has 1 or more bits set.
    /// </summary>
    public static T[] GetValuesExcluding32<T>(params T[] excludes) where T : struct, Enum
    {
        var excludeValues = excludes.Select(exclude => Convert.ToInt64(exclude));
        return Enum.GetValues<T>()
            .Where(e => !excludeValues.Contains(Convert.ToInt64(e)))
            .ToArray();
    }

    /// <summary>
    /// Fields types of the Display attribute.
    /// </summary>
    public enum DisplayType
    {
        Name,
        ShortName,
        GroupName,
        Description,
        Order,
        Value
    }

    /// <summary>
    /// Returns the values of the [DisplayName] attributes for each flag in the enum.
    /// </summary>
    public static string GetDisplayName32(this Enum flags, DisplayType nameType = DisplayType.Name, bool includeAnyMatching = false)
    {
        if (flags == null)
        {
            return string.Empty;
        }

        var names = new HashSet<string>();
        var values = GetDisplayValues(flags.GetType());

        if (BitOperations.PopCount((ulong)Convert.ToInt64(flags)) == 0)
        {
            var noneValue = values.FirstOrDefault(v => BitOperations.PopCount((ulong)Convert.ToInt64(v)) == 0);
            if (noneValue != null)
            {
                return noneValue.GetSingleDisplayName(nameType);
            }

            return string.Empty;
        }

        foreach (var value in values)
        {
            bool isSingleValue = BitOperations.PopCount((ulong)Convert.ToInt64(value)) == 1;
            bool hasNoSingleValue = !values.Any(v => value.HasFlag(v) && flags.HasFlag(v) && BitOperations.PopCount((ulong)Convert.ToInt64(v)) == 1);
            bool hasFlag = includeAnyMatching ? flags.HasAnyFlag32(value) : flags.HasFlag(value);

            if (hasFlag
                // Is a compound value with none of its' values set, or is a single value that is set
                && (isSingleValue || hasNoSingleValue)
                // Skip the None value since flags has something set
                && BitOperations.PopCount((ulong)Convert.ToInt64(value)) > 0)
            {
                names.Add(value.GetSingleDisplayName(nameType));
            }
        }

        return string.Join(", ", names);
    }

    /// <summary>
    /// Returns the value of the [DisplayName] attribute.
    /// </summary>
    public static string GetSingleDisplayName(this Enum @enum, DisplayType nameType = DisplayType.Name)
    {
        var memberInfo = @enum.GetType().GetMember(@enum.ToString());
        if (memberInfo != null && memberInfo.Length > 0)
        {
            var attribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>(true);
            if (attribute != null)
            {
                return nameType switch
                {
                    DisplayType.Name => attribute.GetName(),
                    DisplayType.ShortName => attribute.GetShortName() ?? attribute.GetName(),
                    DisplayType.GroupName => attribute.GetGroupName() ?? attribute.GetShortName() ?? attribute.GetName(),
                    DisplayType.Description => attribute.GetDescription(),
                    DisplayType.Order => attribute.GetOrder().ToString(),
                    _ => null
                } ?? @enum.GetDisplayName32(nameType);
            }
            else
            {
                return memberInfo[0].Name;
            }
        }

        return @enum.GetDisplayName32(nameType);
    }

    /// <summary>
    /// Returns the value of the [DisplayName] attribute.
    /// </summary>
    public static string? GetSingleDisplayNameOrNull(this Enum @enum, DisplayType nameType = DisplayType.Name)
    {
        var memberInfo = @enum.GetType().GetMember(@enum.ToString());
        if (memberInfo != null && memberInfo.Length > 0)
        {
            var attribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>(true);
            if (attribute != null)
            {
                return nameType switch
                {
                    DisplayType.Name => attribute.GetName(),
                    DisplayType.ShortName => attribute.GetShortName() ?? attribute.GetName(),
                    DisplayType.GroupName => attribute.GetGroupName() ?? attribute.GetShortName() ?? attribute.GetName(),
                    DisplayType.Description => attribute.GetDescription(),
                    DisplayType.Order => attribute.GetOrder().ToString(),
                    _ => null
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the value of the [DisplayName] attribute.
    /// </summary>
    public static string GetDisplayName322<T>(this T @enum, DisplayType nameType = DisplayType.Name, DisplayType order = DisplayType.Order, bool includeAny = false) where T : struct, Enum
    {
        var results = new Dictionary<T, string?>();
        foreach (var value in Enum.GetValues<T>().OrderByDescending(e => e.PopCount()))
        {
            // If enum has all the values of the value we are checking.
            if ((Convert.ToInt64(@enum) & Convert.ToInt64(value)) == Convert.ToInt64(value))
            {
                // The value does not have all the values in all of the results.
                if (!((results.Aggregate(0L, (curr, n) => Convert.ToInt64(curr) | Convert.ToInt64(n.Key)) & Convert.ToInt64(value)) == Convert.ToInt64(value))
                    // The value does not have any flags set in any of the results. 
                    && (includeAny || !results.Any(r => (Convert.ToInt64(r.Key) & Convert.ToInt64(value)) > 0)))
                {
                    // Make sure the enum has a display name.
                    if (value.GetSingleDisplayNameOrNull(nameType) is string name)
                    {
                        results.Add(value, name);
                    }
                }
            }
        }

        // None value.
        if (results.Count == 0)
        {
            return @enum.GetDisplayName32(nameType);
        }

        return string.Join(", ", (order switch
        {
            DisplayType.Order => results.OrderBy(r => r.Key.GetSingleDisplayNameOrNull(DisplayType.Order)),
            DisplayType.Name or _ => results.OrderBy(r => r.Value)
        }).ThenBy(r => r.Key).Select(r => r.Value));
    }
}
