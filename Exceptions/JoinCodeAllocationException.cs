namespace USPSimGame.Exceptions;

public class JoinCodeAllocationException : USPSimGameException
{
    public int Attempts { get; }

    public JoinCodeAllocationException(int attempts)
        : base($"Could not allocate a unique join code after {attempts} attempts.")
    {
        Attempts = attempts;
    }
}
