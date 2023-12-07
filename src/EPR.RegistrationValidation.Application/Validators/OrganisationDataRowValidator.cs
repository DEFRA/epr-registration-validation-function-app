namespace EPR.RegistrationValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

[ExcludeFromCodeCoverage]
public class OrganisationDataRowValidator : AbstractValidator<OrganisationDataRow>
{
    public OrganisationDataRowValidator()
    {
        Include(new OrganisationIdValidator());
        Include(new OrganisationNameValidator());
        Include(new HomeNationCodeValidator());
        Include(new PrimaryContactValidator());
        Include(new RegisteredAddressValidator());
    }
}