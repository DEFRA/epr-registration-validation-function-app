namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using FluentValidation;

public class OrganisationNameValidator : AbstractValidator<OrganisationDataRow>
{
    public OrganisationNameValidator()
    {
        RuleFor(x => x.OrganisationName)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.MissingOrganisationName);
    }
}