using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Hybrid.Converters;

public sealed class MethodToValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter is not string methodName)
        {
            return value;
        }

        var methodInfo = value.GetType().GetMethod(methodName, Type.EmptyTypes);
        if (methodInfo == null)
        {
            methodInfo = GetExtensionMethod(value.GetType(), methodName);
            if (methodInfo == null)
            {
                return value;
            }

            return methodInfo.Invoke(null, new[] { value });
        }

        return methodInfo.Invoke(value, Array.Empty<object>());
    }

    static MethodInfo? GetExtensionMethod(Type extendedType, string methodName)
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => !type.IsGenericType && !type.IsNested)
            .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), (_, method) => method)
            .Where(m => m.IsDefined(typeof(ExtensionAttribute), false))
            .Where(m => m.GetParameters()[0].ParameterType == extendedType)
            .FirstOrDefault(m => m.Name == methodName);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("MethodToValueConverter can only be used for one way conversion.");
    }
}