
namespace Core.Code.Exceptions;

[Serializable]
public class UserException : ArgumentException
{
    public UserException() { }

    public UserException(string message) : base(message) { }

    public UserException(string message, Exception innerException) : base(message, innerException) { }
}
