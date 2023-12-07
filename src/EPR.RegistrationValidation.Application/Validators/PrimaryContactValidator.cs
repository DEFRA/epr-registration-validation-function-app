namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using Extensions;
using FluentValidation;

public class PrimaryContactValidator : AbstractValidator<OrganisationDataRow>
{
    public PrimaryContactValidator()
    {
        RuleFor(x => x.PrimaryContactPersonFirstName)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.MissingPrimaryContactFirstName);
        RuleFor(x => x.PrimaryContactPersonLastName)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.MissingPrimaryContactLastName);
        RuleFor(x => x.PrimaryContactPersonPhoneNumber)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.MissingPrimaryContactPhoneNumber);
        RuleFor(x => x.PrimaryContactPersonEmail)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.MissingPrimaryContactEmail);
    }
}