namespace EPR.RegistrationValidation.Application.Validators;

using System.Globalization;
using Data.Constants;
using Data.Models;
using EPR.RegistrationValidation.Application.Services.HelperFunctions;
using FluentValidation;

public class TotalTonnageValidator : AbstractValidator<OrganisationDataRow>
{
    public TotalTonnageValidator()
    {
        RuleFor(x => x.TotalTonnage)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.TotalTonnageMustBeProvided);

        RuleFor(x => x.TotalTonnage)
            .Cascade(CascadeMode.StopOnFirstFailure)
            .Must(NotContainComma).WithErrorCode(ErrorCodes.TotalTonnageIncludesComma)
            .Must(BeNumeric).WithErrorCode(ErrorCodes.TotalTonnageIsNotNumber)
            .Must(BeGreaterThanZero).WithErrorCode(ErrorCodes.TotalTonnageMustBeGreaterThanZero)
            .When(orgRow => !string.IsNullOrEmpty(orgRow.TotalTonnage) && !HelperFunctions.HasMetZeroReturnYesNoCondition(orgRow));
    }

    private static bool BeNumeric(string number)
    {
        return decimal.TryParse(
            number,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out _);
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
}