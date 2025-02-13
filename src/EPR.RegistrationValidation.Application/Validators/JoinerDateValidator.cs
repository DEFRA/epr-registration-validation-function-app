namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class JoinerDateValidator : AbstractValidator<OrganisationDataRow>
{
    public JoinerDateValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.JoinerDate)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.SubsidiaryId))
            .WithErrorCode(ErrorCodes.JoinerDateIsRequired);

        RuleFor(r => r.JoinerDate)
            .Must(x => DateTime.TryParseExact(x, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out _))
            .When(x => !string.IsNullOrEmpty(x.JoinerDate))
            .WithErrorCode(ErrorCodes.InvalidJoinerDateFormat);
    }
}
