namespace LogExpert.UI.Extensions;

public class LockFinderException : Exception
{
    public LockFinderException ()
    {

    }

    public LockFinderException (string message) : base(message)
    {

    }

    public LockFinderException (string message, Exception innerException) : base(message, innerException)
    {

    }
}
