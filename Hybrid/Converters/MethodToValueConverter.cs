using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Hybrid.Converters;

public sealed class MethodToValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var methodName = parameter as string;
        if (value == null || methodName == null)
            return value;
        var methodInfo = value.GetType().GetMethod(methodName, Type.EmptyTypes);
        if (methodInfo == null)
        {
            methodInfo = GetExtensionMethod(value.GetType(), methodName);
            if (methodInfo == null) return value;
            return methodInfo.Invoke(null, new[] { value });
        }
        return methodInfo.Invoke(value, Array.Empty<object>());
    }

    static MethodInfo? GetExtensionMethod(Type extendedType, string methodName)
    {
        var method = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => !type.IsGenericType && !type.IsNested)
            .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), (_, method) => method)
            .Where(m => m.IsDefined(typeof(ExtensionAttribute), false))
            .Where(m => m.GetParameters()[0].ParameterType == extendedType)
            .FirstOrDefault(m => m.Name == methodName);
        return method;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("MethodToValueConverter can only be used for one way conversion.");
    }
}