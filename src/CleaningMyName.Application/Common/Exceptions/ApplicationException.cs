namespace CleaningMyName.Application.Common.Exceptions;

public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message) : base(message)
    {
    }
}
