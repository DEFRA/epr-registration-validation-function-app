namespace EPR.RegistrationValidation.Application.Validators;

using Constants;
using Data.Constants;
using Data.Models;
using FluentValidation;

public class ClosedLoopRegistrationValidator : AbstractValidator<OrganisationDataRow>
{
    public ClosedLoopRegistrationValidator()
    {
        // Column is optional — if absent (null) the row is valid.
        // If the column is present, a non-empty Yes/No value is required.
        When(row => row.ClosedLoopRegistration != null, () =>
        {
            RuleFor(row => row.ClosedLoopRegistration)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithErrorCode(ErrorCodes.InvalidClosedLoopRegistrationValue)
                .Must(v => v.Equals(YesNoOption.Yes, StringComparison.OrdinalIgnoreCase)
                        || v.Equals(YesNoOption.No, StringComparison.OrdinalIgnoreCase))
                .WithErrorCode(ErrorCodes.InvalidClosedLoopRegistrationValue);
        });
    }
}
