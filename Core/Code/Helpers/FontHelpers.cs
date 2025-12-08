
namespace Core.Code.Helpers;

public static class FontHelpers
{
    private static IDictionary<string, double> FontSizeEms = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
    {
        ["xx-small"] = 0.7,
        ["x-small"] = 0.8,
        ["smaller"] = 0.9,
    };

    private static double Adjust(double fontSizeEm, int? adjustBy = null)
    {
        if (adjustBy.HasValue)
        {
            return fontSizeEm + (adjustBy.Value * 0.1);
        }

        return fontSizeEm;
    }

    public static string AdjustEm(double fontSizeEm, int? adjustBy = null)
    {
        return Adjust(fontSizeEm, adjustBy).ToString("F2").Trim('0') + "em";
    }

    public static string AdjustEm(string fontSize, int? adjustBy = null)
    {
        return Adjust(FontSizeEms[fontSize], adjustBy).ToString("F2").Trim('0') + "em";
    }
}
