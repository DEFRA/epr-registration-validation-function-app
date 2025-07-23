namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class AbstractCodeValidator : AbstractValidator<OrganisationDataRow>
{
    public AbstractCodeValidator(bool uploadedByComplianceScheme, bool enableLeaverCodeValidation)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        if (!enableLeaverCodeValidation)
        {
            RuleFor(r => r.LeaverCode)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverDate))
            .WithErrorCode(ErrorCodes.StatusCodeMustBePresentWhenLeaverDatePresent);

            RuleFor(r => r.LeaverCode)
                .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.SubsidiaryId) && uploadedByComplianceScheme && !string.IsNullOrEmpty(x.LeaverDate))
                .WithErrorCode(ErrorCodes.StatusCodeMustBePresentWhenLeaverDatePresentCS);
        }
    }
}
