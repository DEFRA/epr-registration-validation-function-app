namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using Extensions;
using FluentValidation;

public class OrganisationIdValidator : AbstractValidator<OrganisationDataRow>
{
    public OrganisationIdValidator()
    {
        RuleFor(x => x.DefraId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.MissingOrganisationId);
    }
}