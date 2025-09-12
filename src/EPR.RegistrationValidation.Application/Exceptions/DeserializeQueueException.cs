namespace EPR.RegistrationValidation.Application.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public class DeserializeQueueException : Exception
{
    public DeserializeQueueException()
    {
    }

    public DeserializeQueueException(string message)
        : base(message)
    {
    }

    public DeserializeQueueException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected DeserializeQueueException(SerializationInfo info, StreamingContext context)
#pragma warning disable SYSLIB0051 // Type or member is obsolete
        : base(info, context)
#pragma warning restore SYSLIB0051 // Type or member is obsolete
    {
    }
}