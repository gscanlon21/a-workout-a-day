using System.Globalization;

namespace Hybrid.Converters;

public sealed class SelectionChangedEventArgsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SelectionChangedEventArgs args && args.CurrentSelection.Count > 0)
        {
            return args.CurrentSelection[0];
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}