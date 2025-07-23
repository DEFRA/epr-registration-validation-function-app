namespace EPR.RegistrationValidation.Application.Validators;

using System;
using System.Text.RegularExpressions;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class RegistrationTypeCodeValidator : AbstractValidator<OrganisationDataRow>
{
    public RegistrationTypeCodeValidator(bool uploadedByComplianceScheme)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.RegistrationTypeCode)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.SubsidiaryId))
            .WithErrorCode(ErrorCodes.RegistrationTypeCodeIsMandatory);

        RuleFor(r => r.RegistrationTypeCode)
            .NotEmpty()
            .When(x => string.IsNullOrEmpty(x.SubsidiaryId) && uploadedByComplianceScheme)
            .WithErrorCode(ErrorCodes.RegistrationTypeCodeIsMandatoryCS);

        RuleFor(r => r.RegistrationTypeCode)
            .Must(x => Regex.IsMatch(x, "^(GR|IN)$", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
            .When(x => !string.IsNullOrEmpty(x.RegistrationTypeCode))
            .WithErrorCode(ErrorCodes.RegistrationTypeCodeInvalidValue);

        RuleFor(r => r.RegistrationTypeCode)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.LeaverCode))
            .WithErrorCode(ErrorCodes.RegistrationTypeCodeIsMandatory);
    }
}
