namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

/// <summary>
/// Validator for Standard Industrial Classification (SIC) Code
/// https://www.gov.uk/government/publications/standard-industrial-classification-of-economic-activities-sic.
/// </summary>
public class OrganisationMainActivitySicValidator : AbstractValidator<OrganisationDataRow>
{
    public OrganisationMainActivitySicValidator()
    {
        RuleFor(organisation => organisation.MainActivitySic)
            .Must(BeFiveDigitsNumber)
            .When(organisation => !string.IsNullOrEmpty(organisation.MainActivitySic))
            .WithErrorCode(ErrorCodes.MainActivitySicNotFiveDigitsInteger);
    }

    public static bool BeFiveDigitsNumber(string number)
    {
        return
            !string.IsNullOrEmpty(number) &&
            number.Length == 5 &&
            number.All(digit => digit is >= '0' and <= '9');
    }
}