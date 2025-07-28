namespace EPR.RegistrationValidation.Application.Validators;

using System.Globalization;
using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class JoinerDateValidator : AbstractValidator<OrganisationDataRow>
{
    public JoinerDateValidator(bool uploadedByComplianceScheme, bool enableAdditionalValidation, bool enableLeaverCodeValidation)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        if (enableLeaverCodeValidation)
        {
            RuleFor(r => r.JoinerDate)
                .NotEmpty()
                .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && (x.LeaverCode == JoinerCode.LeaverCode02 || x.LeaverCode == JoinerCode.LeaverCode03))
                .WithErrorCode(ErrorCodes.JoinerDateIsMandatoryDP);

            RuleFor(r => r.JoinerDate)
                .Empty()
                .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && LeaverCodeIsValid(x.LeaverCode))
                .WithErrorCode(ErrorCodes.JoinerdateNotAllowedWhenLeaverCodeIsPresent);
        }
        else
        {
            if (enableAdditionalValidation)
            {
                RuleFor(r => r.JoinerDate)
               .NotEmpty()
               .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && (x.LeaverCode == StatusCode.B || x.LeaverCode == StatusCode.C))
               .WithErrorCode(ErrorCodes.JoinerDateIsMandatoryDP);

                RuleFor(r => r.JoinerDate)
                   .NotEmpty()
                   .When(x => string.IsNullOrEmpty(x.SubsidiaryId) && uploadedByComplianceScheme && (x.LeaverCode == StatusCode.B || x.LeaverCode == StatusCode.C))
                   .WithErrorCode(ErrorCodes.JoinerDateIsMandatoryCS);
            }
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
            leaverCode == LeaverCode.LeaverCode16;
    }
}