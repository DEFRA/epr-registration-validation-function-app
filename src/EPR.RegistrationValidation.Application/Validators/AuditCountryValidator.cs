namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using EPR.RegistrationValidation.Application.Constants;
using FluentValidation;

public class AuditCountryValidator : AbstractValidator<OrganisationDataRow>
{
    private static readonly string[] _codes =
    {
        AuditingCountryCodes.England,
        AuditingCountryCodes.NorthernIreland,
        AuditingCountryCodes.Scotland,
        AuditingCountryCodes.Wales,
        AuditingCountryCodes.En,
        AuditingCountryCodes.Uk,
        AuditingCountryCodes.Ws,
        AuditingCountryCodes.Sc,
        AuditingCountryCodes.Ni,
        AuditingCountryCodes.Gb,
        AuditingCountryCodes.GreatBritain,
    };

    public AuditCountryValidator()
    {
        RuleFor(x => x.AuditAddressCountry)
            .Must(code => _codes.Contains(code, StringComparer.OrdinalIgnoreCase))
            .When(row => !string.IsNullOrWhiteSpace(row.AuditAddressCountry))
            .WithErrorCode(ErrorCodes.InvalidAuditAddressCountry);
    }
}