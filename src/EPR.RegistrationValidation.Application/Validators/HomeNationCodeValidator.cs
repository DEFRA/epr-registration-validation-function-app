namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using FluentValidation;

public class HomeNationCodeValidator : AbstractValidator<OrganisationDataRow>
{
    private static readonly string[] ValidHomeCodes = { "EN", "NI", "SC", "WS" };

    public HomeNationCodeValidator()
    {
        RuleFor(x => x.HomeNationCode)
            .NotEmpty().WithErrorCode(ErrorCodes.MissingHomeNationCode)
            .Must(code => ValidHomeCodes.Contains(code))
            .WithErrorCode(ErrorCodes.InvalidHomeNationCode);
    }
}