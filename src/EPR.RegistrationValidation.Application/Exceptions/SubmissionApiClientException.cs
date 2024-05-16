namespace EPR.RegistrationValidation.Application.Exceptions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

[ExcludeFromCodeCoverage]
[Serializable]
public class SubmissionApiClientException : Exception
{
    public SubmissionApiClientException()
    {
    }

    public SubmissionApiClientException(string message)
        : base(message)
    {
    }

    public SubmissionApiClientException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected SubmissionApiClientException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
