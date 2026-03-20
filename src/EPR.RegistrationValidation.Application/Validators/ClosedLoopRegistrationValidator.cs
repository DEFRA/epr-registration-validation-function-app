namespace EPR.RegistrationValidation.Application.Validators;

using Constants;
using Data.Constants;
using Data.Models;
using FluentValidation;

public class ClosedLoopRegistrationValidator : AbstractValidator<OrganisationDataRow>
{
    public ClosedLoopRegistrationValidator()
    {
        // Column is optional — only validate when a value is provided
        When(row => !string.IsNullOrEmpty(row.ClosedLoopRegistration), () =>
        {
            RuleFor(row => row.ClosedLoopRegistration)
                .Must(v => v.Equals(YesNoOption.Yes, StringComparison.OrdinalIgnoreCase)
                        || v.Equals(YesNoOption.No, StringComparison.OrdinalIgnoreCase))
                .WithErrorCode(ErrorCodes.InvalidClosedLoopRegistrationValue);
        });
    }
}
