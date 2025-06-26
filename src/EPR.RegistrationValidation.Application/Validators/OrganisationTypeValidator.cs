namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class OrganisationTypeValidator : AbstractValidator<OrganisationDataRow>
{
    private static readonly string[] _codes =
    {
        UnIncorporationTypeCodes.CoOperative,
        UnIncorporationTypeCodes.SoleTrader,
        UnIncorporationTypeCodes.Partnership,
        UnIncorporationTypeCodes.Others,
        UnIncorporationTypeCodes.OutsideUk,
        OrganisationTypeCode.Regulator,
        IncorporationTypeCodes.LimitedCompany,
        IncorporationTypeCodes.LimitedLiabilityPartnership,
        IncorporationTypeCodes.LimitedPartnership,
        IncorporationTypeCodes.PublicLimitedCompany,
        HybridCorporationType.CommunityInterestCompany,
    };

    public OrganisationTypeValidator()
    {
        RuleFor(x => x.OrganisationTypeCode)
            .NotEmpty().WithErrorCode(ErrorCodes.MissingOrganisationTypeCode);

        RuleFor(x => x.OrganisationTypeCode)
            .Must(code => _codes.Contains(code, StringComparer.OrdinalIgnoreCase))
            .When(row => !string.IsNullOrWhiteSpace(row.OrganisationTypeCode))
            .WithErrorCode(ErrorCodes.InvalidOrganisationTypeCode);
    }
}