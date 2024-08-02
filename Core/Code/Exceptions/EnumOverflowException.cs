using System.Collections;
using System.Text;

[Serializable]
public class EnumOverflowException : OverflowException
{
    public EnumOverflowException() { }

    public EnumOverflowException(string message) : base(message) { }

    public EnumOverflowException(string message, Exception innerException) : base(message, innerException) { }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(base.ToString());

        Exception? exception = this;
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
