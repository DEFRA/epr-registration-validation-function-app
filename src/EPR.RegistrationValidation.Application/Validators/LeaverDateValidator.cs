namespace EPR.RegistrationValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

[ExcludeFromCodeCoverage]
public class LeaverDateValidator : AbstractValidator<OrganisationDataRow>
{
    public LeaverDateValidator(bool uploadedByComplianceScheme)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.LeaverDate)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverCode))
            .WithErrorCode(ErrorCodes.LeaverDateMustBePresentWhenLeaverCodePresent);

        RuleFor(r => r.LeaverDate)
            .NotEmpty()
            .When(x => string.IsNullOrEmpty(x.SubsidiaryId) && uploadedByComplianceScheme && !string.IsNullOrEmpty(x.LeaverCode))
            .WithErrorCode(ErrorCodes.LeaverDateMustBePresentWhenLeaverCodePresentCS);

        RuleFor(r => r.LeaverDate)
            .Must(x => DateFormatIsValid(x))
            .When(x => !string.IsNullOrEmpty(x.LeaverDate))
            .WithErrorCode(ErrorCodes.InvalidLeaverDateFormat);

        RuleFor(r => r.LeaverDate)
            .Must(x => DateTime.TryParseExact(x, "dd/MM/yyyy", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var leaverDate) &&
                leaverDate.Date <= DateTime.Now.Date)
            .When(x => !string.IsNullOrEmpty(x.LeaverDate) && DateFormatIsValid(x.LeaverDate))
            .WithErrorCode(ErrorCodes.LeaverDateCannotBeInTheFuture);

        RuleFor(r => r.LeaverDate)
            .Must((r, x) => DateTime.TryParseExact(r.JoinerDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var joinerDate) &&
                DateTime.TryParseExact(r.LeaverDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var leaverDate) &&
                joinerDate.Date <= leaverDate.Date)
            .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) &&
                !string.IsNullOrEmpty(x.JoinerDate) &&
                !string.IsNullOrEmpty(x.LeaverDate) &&
                DateFormatIsValid(x.JoinerDate) &&
                DateFormatIsValid(x.LeaverDate))
            .WithErrorCode(ErrorCodes.LeaverDateMustBeAfterJoinerDate);

        RuleFor(r => r.LeaverDate)
           .Must((r, x) => DateTime.TryParseExact(r.JoinerDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var joinerDate) &&
               DateTime.TryParseExact(r.LeaverDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var leaverDate) &&
               joinerDate.Date <= leaverDate.Date)
           .When(x => string.IsNullOrEmpty(x.SubsidiaryId) &&
               uploadedByComplianceScheme &&
               !string.IsNullOrEmpty(x.JoinerDate) &&
               !string.IsNullOrEmpty(x.LeaverDate) &&
               DateFormatIsValid(x.JoinerDate) &&
               DateFormatIsValid(x.LeaverDate))
           .WithErrorCode(ErrorCodes.LeaverDateMustBeAfterJoinerDateCS);
    }

    private bool DateFormatIsValid(string date)
    {
        return DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _);
    }
}
