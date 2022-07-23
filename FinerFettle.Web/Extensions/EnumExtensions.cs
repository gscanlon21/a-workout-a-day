using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FinerFettle.Web.Extensions
{
    public static class EnumExtensions
    {
        public static IEnumerable<T> GetFlags<T>(this T flags) where T : Enum
        {
            return Enum.GetValues(flags.GetType()).Cast<T>().Where(e => flags.HasFlag(e));
        }

        public static bool HasAnyFlag32<T>(this T flags, T oneOf) where T : Enum
        {
            return (Convert.ToInt32(flags) & Convert.ToInt32(oneOf)) != 0;
        }

        public static T UnsetFlag32<T>(this T flags, T unset) where T : Enum
        {
            return (T)(object)(Convert.ToInt32(flags) & ~Convert.ToInt32(unset));
        }

        public static T Next<T>(this T flags) where T : Enum
        {
            var values = (T[])Enum.GetValues(flags.GetType());
            var nextIdx = Array.IndexOf(values, flags) + 1;
            return values.Length == nextIdx ? values[0] : values[nextIdx];
        }

        public static string GetDisplayName32(this Enum flags)
        {
            if (flags == null)
            {
                return String.Empty;
            }

            var names = new List<string>();
            foreach (var value in Enum.GetValues(flags.GetType()).Cast<Enum>().Where(e => Convert.ToInt32(e) > 0))
            {
                if (flags.HasFlag(value))
                {
                    names.Add(GetSingleDisplayName(value));
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

        public static string GetSingleDisplayName(this Enum @enum)
        {
            var memberInfo = @enum.GetType().GetMember(@enum.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DisplayAttribute)attrs[0]).Name ?? @enum.ToString();
                }
            }

            return @enum.ToString();
        }
    }
}
