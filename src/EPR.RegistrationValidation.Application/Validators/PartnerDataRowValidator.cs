namespace EPR.RegistrationValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

[ExcludeFromCodeCoverage]
public class PartnerDataRowValidator : AbstractValidator<PartnersDataRow>
{
    public PartnerDataRowValidator()
    {
        Include(new PartnerDataRowCharacterLengthValidator());
        Include(new PartnerDataRowOrganisationDataValidator());
    }
}