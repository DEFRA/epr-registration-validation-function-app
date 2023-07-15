namespace EPR.RegistrationValidation.Application.Exceptions;

[Serializable]
public class CsvParseException : Exception
{
    public CsvParseException()
    {
    }

    public CsvParseException(string message)
        : base(message)
    {
    }

    public CsvParseException(string message, Exception inner)
        : base(message, inner)
    {
    }
}