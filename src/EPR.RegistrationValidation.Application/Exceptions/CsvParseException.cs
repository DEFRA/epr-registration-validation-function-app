namespace EPR.RegistrationValidation.Application.Exceptions;

using System.Runtime.Serialization;

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

    protected CsvParseException(SerializationInfo info, StreamingContext context)
#pragma warning disable SYSLIB0051 // Type or member is obsolete
        : base(info, context)
#pragma warning restore SYSLIB0051 // Type or member is obsolete
    {
    }
}