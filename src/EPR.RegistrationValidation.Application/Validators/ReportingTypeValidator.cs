namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class ReportingTypeValidator : AbstractValidator<OrganisationDataRow>
{
    public ReportingTypeValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.ReportingType)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.SubsidiaryId))
            .WithErrorCode(ErrorCodes.ReportingTypeIsRequired);

        RuleFor(r => r.ReportingType)
            .Must(x => x.Equals(ReportingType.Self, StringComparison.InvariantCultureIgnoreCase) || x.Equals(ReportingType.Group, StringComparison.InvariantCultureIgnoreCase))
            .When(x => !string.IsNullOrEmpty(x.ReportingType))
            .WithErrorCode(ErrorCodes.InvalidReportingType);
    }
}
