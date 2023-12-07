namespace EPR.RegistrationValidation.Application.Exceptions;

public class CsvHeaderException : Exception
{
    public CsvHeaderException()
    {
    }

    public CsvHeaderException(string message)
        : base(message)
    {
    }

    public CsvHeaderException(string message, Exception inner)
        : base(message, inner)
    {
    }
}