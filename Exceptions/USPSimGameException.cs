namespace USPSimGame.Exceptions;

public abstract class USPSimGameException : Exception
{
    protected USPSimGameException(string message) : base(message)
    {
    }

    protected USPSimGameException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
