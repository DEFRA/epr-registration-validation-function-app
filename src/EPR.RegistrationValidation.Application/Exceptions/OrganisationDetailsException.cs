namespace EPR.RegistrationValidation.Application.Exceptions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

[ExcludeFromCodeCoverage]
[Serializable]
public class OrganisationDetailsException : Exception
{
    public OrganisationDetailsException()
    {
    }

    public OrganisationDetailsException(string message)
        : base(message)
    {
    }

    public OrganisationDetailsException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
