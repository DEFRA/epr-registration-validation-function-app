namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using FluentValidation;

public class RegisteredAddressValidator : AbstractValidator<OrganisationDataRow>
{
    private static readonly string[] ChOrganisationCodes = { "LLP", "LTD", "LP" };

    public RegisteredAddressValidator()
    {
        RuleFor(x => x.RegisteredAddressLine1)
            .NotEmpty()
            .When(x => ChOrganisationCodes.Contains(x.OrganisationTypeCode))
            .WithErrorCode(ErrorCodes.MissingRegisteredAddressLine1);
        RuleFor(x => x.RegisteredAddressPostcode)
            .NotEmpty()
            .When(x => ChOrganisationCodes.Contains(x.OrganisationTypeCode))
            .WithErrorCode(ErrorCodes.MissingRegisteredAddressPostcode);
        RuleFor(x => x.RegisteredAddressPhoneNumber)
            .NotEmpty()
            .When(x => ChOrganisationCodes.Contains(x.OrganisationTypeCode))
            .WithErrorCode(ErrorCodes.MissingRegisteredAddressPhoneNumber);
    }
}