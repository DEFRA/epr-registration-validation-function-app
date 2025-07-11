namespace EPR.RegistrationValidation.Application.Exceptions;

using System;
using System.Diagnostics.CodeAnalysis;

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
}
