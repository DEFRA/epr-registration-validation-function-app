namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class LeaverDateValidator : AbstractValidator<OrganisationDataRow>
{
    public LeaverDateValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.LeaverDate)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverCode))
            .WithErrorCode(ErrorCodes.LeaverDateMustBePresentWhenLeaverCodePresent);

        RuleFor(r => r.LeaverDate)
            .Must(x => DateTime.TryParseExact(x, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out _))
            .When(x => !string.IsNullOrEmpty(x.LeaverDate))
            .WithErrorCode(ErrorCodes.InvalidLeaverDateFormat);

        RuleFor(r => r.LeaverDate)
            .Must((r, x) => DateTime.TryParseExact(r.JoinerDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var joinerDate) &&
                DateTime.TryParseExact(r.LeaverDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var leaverDate) &&
                joinerDate.Date <= leaverDate.Date)
            .When(x => !string.IsNullOrEmpty(x.JoinerDate) &&
                !string.IsNullOrEmpty(x.LeaverDate) &&
                DateTime.TryParseExact(x.JoinerDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out _) &&
                DateTime.TryParseExact(x.LeaverDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out _))
            .WithErrorCode(ErrorCodes.LeaverDateMustBeAfterJoinerDate);
    }
}
