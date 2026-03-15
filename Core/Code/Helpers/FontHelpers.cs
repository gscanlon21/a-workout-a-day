
namespace Core.Code.Helpers;

public static class FontHelpers
{
    private static readonly Dictionary<string, double> FontSizeEms = new(StringComparer.OrdinalIgnoreCase)
    {
        ["xx-small"] = 0.7,
        ["x-small"] = 0.8,
        ["smaller"] = 0.9,
    };

    private static double Adjust(double fontSizeEm, double? min = null)
    {
        if (min.HasValue)
        {
            return Math.Max(fontSizeEm, min.Value);
        }

        return fontSizeEm;
    }

    public static string AdjustEm(double fontSizeEm, double? min = null)
    {
        return Adjust(fontSizeEm, min).ToString("F2").Trim('0') + "em";
    }

    public static string AdjustEm(string fontSize, double? min = null)
    {
        return Adjust(FontSizeEms[fontSize], min).ToString("F2").Trim('0') + "em";
    }
}
