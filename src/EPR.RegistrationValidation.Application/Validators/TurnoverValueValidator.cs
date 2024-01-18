namespace EPR.RegistrationValidation.Application.Validators;

using System.Globalization;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class TurnoverValueValidator : AbstractValidator<OrganisationDataRow>
{
    public TurnoverValueValidator()
    {
        RuleFor(turnoverValue => turnoverValue.Turnover)
            .Must(NotContainComma).WithErrorCode(ErrorCodes.TurnoverHasComma);

        RuleFor(turnoverValue => turnoverValue.Turnover)
            .Must(BeGreaterThanZero).WithErrorCode(ErrorCodes.TurnoverHasZeroOrNegativeValue);

        RuleFor(turnoverValue => turnoverValue.Turnover)
            .Must(BeNumeric).WithErrorCode(ErrorCodes.InvalidTurnoverDigits);

        RuleFor(turnoverValue => turnoverValue.Turnover)
            .Must(BeMaxTwoDecimalPlaces).WithErrorCode(ErrorCodes.InvalidTurnoverDecimalValues);
    }

    private static bool NotContainComma(string number)
    {
        return !string.IsNullOrEmpty(number) &&
              !number.Contains(',');
    }

    private static bool BeGreaterThanZero(string number)
    {
        return decimal.TryParse(number, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal decimalValue) && decimalValue > 0;
    }

    private static bool BeMaxTwoDecimalPlaces(string number)
    {
        if (decimal.TryParse(number, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal decimalValue))
        {
            return decimalValue == decimal.Round(decimalValue, 2);
        }

        return false;
    }

    private static bool BeNumeric(string number)
    {
        return decimal.TryParse(number, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _);
    }
}