namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using FluentValidation;

public class OrganisationTradingNameValidator : AbstractValidator<OrganisationDataRow>
{
    public OrganisationTradingNameValidator()
    {
        RuleFor(oragnisation => oragnisation.TradingName)
            .NotEqual(organisation => organisation.OrganisationName)
            .When(organisation => !string.IsNullOrEmpty(organisation.OrganisationName))
            .WithErrorCode(ErrorCodes.TradingNameSameAsOrganisationName);
    }
}