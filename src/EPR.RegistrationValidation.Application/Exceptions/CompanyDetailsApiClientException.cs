namespace EPR.RegistrationValidation.Application.Exceptions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

[ExcludeFromCodeCoverage]
[Serializable]
public class CompanyDetailsApiClientException : Exception
{
    public CompanyDetailsApiClientException()
    {
    }

    public CompanyDetailsApiClientException(string message)
        : base(message)
    {
    }

    public CompanyDetailsApiClientException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected CompanyDetailsApiClientException(SerializationInfo info, StreamingContext context)
#pragma warning disable SYSLIB0051 // Type or member is obsolete
        : base(info, context)
#pragma warning restore SYSLIB0051 // Type or member is obsolete
    {
    }
}
