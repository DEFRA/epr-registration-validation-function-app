namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class StatusCodeValidator : AbstractValidator<OrganisationDataRow>
{
    public StatusCodeValidator(bool uploadedByComplianceScheme)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.StatusCode)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverDate))
            .WithErrorCode(ErrorCodes.StatusCodeMustBePresentWhenLeaverDatePresent);

        RuleFor(r => r.StatusCode)
            .NotEmpty()
            .When(x => string.IsNullOrEmpty(x.SubsidiaryId) && uploadedByComplianceScheme && !string.IsNullOrEmpty(x.LeaverDate))
            .WithErrorCode(ErrorCodes.StatusCodeMustBePresentWhenLeaverDatePresentCS);

        RuleFor(r => r.StatusCode)
            .Must(x => StatusCodeIsValid(x))
            .When(r => !string.IsNullOrEmpty(r.StatusCode))
            .WithErrorCode(ErrorCodes.InvalidStatusCode);
    }

    private bool StatusCodeIsValid(string statusCode)
    {
        return statusCode == StatusCode.A ||
            statusCode == StatusCode.B ||
            statusCode == StatusCode.C ||
            statusCode == StatusCode.D ||
            statusCode == StatusCode.E ||
            statusCode == StatusCode.F ||
            statusCode == StatusCode.G ||
            statusCode == StatusCode.H ||
            statusCode == StatusCode.I ||
            statusCode == StatusCode.J ||
            statusCode == StatusCode.K ||
            statusCode == StatusCode.L ||
            statusCode == StatusCode.M ||
            statusCode == StatusCode.N ||
            statusCode == StatusCode.O ||
            statusCode == StatusCode.P ||
            statusCode == StatusCode.Q;
    }
}
