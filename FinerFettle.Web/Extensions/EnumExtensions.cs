using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace FinerFettle.Web.Extensions;

public static class EnumExtensions
{
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

        var names = new List<string>();
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

        return String.Join(", ", names.Distinct());
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
                    DisplayNameType.ShortName => attribute.GetShortName(),
                    DisplayNameType.Description => attribute.GetDescription(),
                    DisplayNameType.GroupName => attribute.GetGroupName(),
                    _ => null
                } ?? @enum.ToString();
            }
        }

        return @enum.ToString();
    }
}
