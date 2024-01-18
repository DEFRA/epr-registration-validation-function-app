namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using EPR.RegistrationValidation.Application.Constants;
using FluentValidation;

public class AuditAddressValidator : AbstractValidator<OrganisationDataRow>
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

    public AuditAddressValidator()
    {
        RuleFor(x => x.AuditAddressCountry)
            .Must(code => _codes.Contains(code, StringComparer.OrdinalIgnoreCase))
            .When(row => !string.IsNullOrWhiteSpace(row.AuditAddressCountry))
            .WithErrorCode(ErrorCodes.InvalidAuditAddressCountry);

        RuleFor(x => x.AuditAddressLine1)
            .NotEmpty()
            .When(OtherAddressFieldExists)
            .WithErrorCode(ErrorCodes.MissingAuditAddressLine1);

        RuleFor(x => x.AuditAddressPostcode)
            .NotEmpty()
            .When(OtherAddressFieldExists)
            .WithErrorCode(ErrorCodes.MissingAuditPostcode);
    }

    private static bool OtherAddressFieldExists(OrganisationDataRow row)
    {
        return !string.IsNullOrEmpty(row.AuditAddressLine1) ||
               !string.IsNullOrEmpty(row.AuditAddressLine2) ||
               !string.IsNullOrEmpty(row.AuditAddressCity) ||
               !string.IsNullOrEmpty(row.AuditAddressCounty) ||
               !string.IsNullOrEmpty(row.AuditAddressCountry) ||
               !string.IsNullOrEmpty(row.AuditAddressPostcode);
    }
}