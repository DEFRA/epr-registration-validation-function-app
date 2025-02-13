namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class LeaverReasonValidator : AbstractValidator<OrganisationDataRow>
{
    public LeaverReasonValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.LeaverReason)
            .Must(x => x.Length <= 200)
            .When(x => !string.IsNullOrEmpty(x.LeaverReason))
            .WithErrorCode(ErrorCodes.LeaverReasonExceedsTwoHundredCharacterLimit);
    }
}
