namespace EPR.RegistrationValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

[ExcludeFromCodeCoverage]
public class LeaverDateValidator : AbstractValidator<OrganisationDataRow>
{
    public LeaverDateValidator(bool uploadedByComplianceScheme, bool enableLeaverCodeValidation)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        if (enableLeaverCodeValidation)
        {
            RuleFor(r => r.LeaverDate)
            .Empty()
            .When(x => JoinerCodeIsValid(x.LeaverCode))
            .WithErrorCode(ErrorCodes.LeaverDateShouldNotBePresent);

            RuleFor(r => r.LeaverDate)
            .NotEmpty()
            .When(x => LeaverCodeIsValid(x.LeaverCode))
            .WithErrorCode(ErrorCodes.LeaverDateIsMandatoryForThisCode);
        }
        else
        {
            RuleFor(r => r.LeaverDate)
           .NotEmpty()
           .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverCode))
           .WithErrorCode(ErrorCodes.LeaverDateMustBePresentWhenStatusCodePresent);

            RuleFor(r => r.LeaverDate)
                .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.SubsidiaryId) && uploadedByComplianceScheme && !string.IsNullOrEmpty(x.LeaverCode))
                .WithErrorCode(ErrorCodes.LeaverDateMustBePresentWhenStatusCodePresentCS);

            RuleFor(r => r.LeaverDate)
                .Must(x => DateFormatIsValid(x))
                .When(x => !string.IsNullOrEmpty(x.LeaverDate))
                .WithErrorCode(ErrorCodes.InvalidLeaverDateFormat);
        }

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

    private static bool JoinerCodeIsValid(string joinerCode)
    {
        return joinerCode == JoinerCode.LeaverCode01 ||
            joinerCode == JoinerCode.LeaverCode02 ||
            joinerCode == JoinerCode.LeaverCode03 ||
            joinerCode == JoinerCode.LeaverCode07 ||
            joinerCode == JoinerCode.LeaverCode09 ||
            joinerCode == JoinerCode.LeaverCode15 ||
            joinerCode == JoinerCode.LeaverCode17;
    }

    private static bool LeaverCodeIsValid(string leaverCode)
    {
        return leaverCode == LeaverCode.LeaverCode04 ||
            leaverCode == LeaverCode.LeaverCode05 ||
            leaverCode == LeaverCode.LeaverCode06 ||
            leaverCode == LeaverCode.LeaverCode08 ||
            leaverCode == LeaverCode.LeaverCode10 ||
            leaverCode == LeaverCode.LeaverCode11 ||
            leaverCode == LeaverCode.LeaverCode12 ||
            leaverCode == LeaverCode.LeaverCode13 ||
            leaverCode == LeaverCode.LeaverCode14 ||
            leaverCode == LeaverCode.LeaverCode16 ||
            leaverCode == LeaverCode.LeaverCode21;
    }

    private static bool DateFormatIsValid(string date)
    {
        return DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _);
    }
}