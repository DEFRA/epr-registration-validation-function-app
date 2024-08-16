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
}
