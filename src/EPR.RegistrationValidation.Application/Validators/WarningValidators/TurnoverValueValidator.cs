namespace EPR.RegistrationValidation.Application.Validators.WarningValidators;

using EPR.RegistrationValidation.Application.Services.HelperFunctions;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class TurnoverValueValidator : AbstractValidator<OrganisationDataRow>
{
    public TurnoverValueValidator()
    {
        RuleFor(turnoverValue => turnoverValue.Turnover)
           .Empty() // make it fail to get required 'warning code' as WHEN conditions are true.
           .WithErrorCode(ErrorCodes.WarningZeroTurnover)
           .When(row => HelperFunctions.HasMetZeroReturnYesNoCondition(row) && IsEqualToZero(row.Turnover) && HelperFunctions.IsGreaterThanOrEqualToZero(row.TotalTonnage));
    }

    private static bool IsEqualToZero(string number)
    {
       return !string.IsNullOrWhiteSpace(number)
            && !number.StartsWith('-')
            && (number.StartsWith('0') || number.Contains("0.0"));
    }
}
