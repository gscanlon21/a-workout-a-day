using Data.Query.Options;
using System.Runtime.CompilerServices;

namespace Data.Code.Exceptions;

[Serializable]
internal class InvalidOptionsException : InvalidOperationException
{
    public InvalidOptionsException() { }

    public InvalidOptionsException(string message) : base(message) { }

    public InvalidOptionsException(string message, Exception innerException) : base(message, innerException) { }

    public static void ThrowIfAlreadySet(IOptions? options, [CallerArgumentExpression(nameof(options))] string optionsName = "")
    {
        if (options != null)
        {
            throw new InvalidOptionsException(optionsName);
        }
    }
}
