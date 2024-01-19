namespace EPR.RegistrationValidation.Application.Validators;

using System.Globalization;
using Data.Constants;
using Data.Models;
using FluentValidation;

public class TotalTonnageValidator : AbstractValidator<OrganisationDataRow>
{
    public TotalTonnageValidator()
    {
        RuleFor(x => x.TotalTonnage)
            .Must(BeNumeric).WithErrorCode(ErrorCodes.TotalTonnageIsNotNumber)
            .Must(NotContainComma).WithErrorCode(ErrorCodes.TotalTonnageIncludesComma)
            .Must(BeGreaterThanZero).WithErrorCode(ErrorCodes.TotalTonnageMustBeGreaterThanZero)
            .When(orgRow => !string.IsNullOrEmpty(orgRow.TotalTonnage));
    }

    private static bool BeNumeric(string number)
    {
        return decimal.TryParse(number, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _);
    }

    private static bool NotContainComma(string number)
    {
        return !number.Contains(',');
    }

    private static bool BeGreaterThanZero(string number)
    {
        return decimal.TryParse(number, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal decimalValue) && decimalValue > 0;
    }
}