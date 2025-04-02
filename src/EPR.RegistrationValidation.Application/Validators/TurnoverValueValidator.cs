namespace EPR.RegistrationValidation.Application.Validators;

using System.Globalization;
using EPR.RegistrationValidation.Application.Services.HelperFunctions;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class TurnoverValueValidator : AbstractValidator<OrganisationDataRow>
{
    public TurnoverValueValidator()
    {
        RuleFor(turnoverValue => turnoverValue.Turnover)
            .Cascade(CascadeMode.StopOnFirstFailure)
            .Must(NotContainComma).WithErrorCode(ErrorCodes.TurnoverHasComma)
            .Must(BeNumeric).WithErrorCode(ErrorCodes.InvalidTurnoverDigits)
            .Must(BeGreaterThanZero).WithErrorCode(ErrorCodes.TurnoverHasZeroOrNegativeValue)
            .When(row => !HelperFunctions.HasMetZeroReturnYesNoCondition(row))
            .Must(BeMaxTwoDecimalPlaces).WithErrorCode(ErrorCodes.InvalidTurnoverDecimalValues)
            .When(orgRow => !string.IsNullOrEmpty(orgRow.Turnover));
    }

    private static bool NotContainComma(string number)
    {
        return !number.Contains(',');
    }

    private static bool BeGreaterThanZero(string number)
    {
        return decimal.TryParse(
            number,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out decimal decimalValue) && decimalValue > 0;
    }

    private static bool BeMaxTwoDecimalPlaces(string number)
    {
        if (decimal.TryParse(
            number,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out decimal decimalValue))
        {
            return decimalValue == decimal.Round(decimalValue, 2);
        }

        return false;
    }

    private static bool BeNumeric(string number)
    {
        return decimal.TryParse(
            number,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out _);
    }
}