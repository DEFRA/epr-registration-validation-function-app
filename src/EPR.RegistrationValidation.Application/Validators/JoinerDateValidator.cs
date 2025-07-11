namespace EPR.RegistrationValidation.Application.Validators;

using System.Globalization;
using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class JoinerDateValidator : AbstractValidator<OrganisationDataRow>
{
    public JoinerDateValidator(bool uploadedByComplianceScheme, bool enableAdditionalValidation)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        if (enableAdditionalValidation)
        {
            RuleFor(r => r.JoinerDate)
           .NotEmpty()
           .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && (x.StatusCode == StatusCode.B || x.StatusCode == StatusCode.C))
           .WithErrorCode(ErrorCodes.JoinerDateIsMandatoryDP);

            RuleFor(r => r.JoinerDate)
               .NotEmpty()
               .When(x => string.IsNullOrEmpty(x.SubsidiaryId) && uploadedByComplianceScheme && (x.StatusCode == StatusCode.B || x.StatusCode == StatusCode.C))
               .WithErrorCode(ErrorCodes.JoinerDateIsMandatoryCS);
        }

        RuleFor(r => r.JoinerDate)
            .Must(x => DateTime.TryParseExact(x, "dd/MM/yyyy", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
            .When(x => !string.IsNullOrEmpty(x.JoinerDate))
            .WithErrorCode(ErrorCodes.InvalidJoinerDateFormat);

        RuleFor(r => r.JoinerDate)
            .Must(x => DateTime.TryParseExact(x, "dd/MM/yyyy", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var joinerDate) &&
                joinerDate.Date <= DateTime.Now.Date)
            .When(x => !string.IsNullOrEmpty(x.JoinerDate) && DateTime.TryParseExact(x.JoinerDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
            .WithErrorCode(ErrorCodes.JoinerDateCannotBeInTheFuture);
    }
}