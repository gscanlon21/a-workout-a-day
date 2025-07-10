using System.Collections;
using System.Text;

namespace Core.Code.Extensions;

public static class ExceptionExtensions
{
    public static string ToStringWithData(this Exception? exception)
    {
        if (exception == null)
        {
            return string.Empty;
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(exception.ToString());

        do
        {
            foreach (DictionaryEntry kvp in exception.Data)
            {
                stringBuilder.AppendLine($"[{kvp.Key}, {kvp.Value}]");
            }
        }
        while ((exception = exception.InnerException) != null);

        return stringBuilder.ToString();
    }
}
