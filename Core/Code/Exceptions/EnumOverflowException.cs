
[Serializable]
public class EnumOverflowException : OverflowException
{
    public EnumOverflowException() { }

    public EnumOverflowException(string message) : base(message) { }

    public EnumOverflowException(string message, Exception innerException) : base(message, innerException) { }
}
