namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using EPR.RegistrationValidation.Application.Constants;
using FluentValidation;

public class PrincipalAddressValidator : AbstractValidator<OrganisationDataRow>
{
    private static readonly string[] _codes =
    {
        UnIncorporationTypeCodes.CoOperative,
        UnIncorporationTypeCodes.SoleTrader,
        UnIncorporationTypeCodes.Partnership,
        UnIncorporationTypeCodes.Others,
        UnIncorporationTypeCodes.OutsideUk,
    };

    public PrincipalAddressValidator()
    {
        RuleFor(x => x.PrincipalAddressLine1)
            .NotEmpty()
            .When(x => _codes.Contains(x.OrganisationTypeCode))
            .WithErrorCode(ErrorCodes.MissingPrincipalAddressLine1);
        RuleFor(x => x.PrincipalAddressPostcode)
            .NotEmpty()
            .When(x => _codes.Contains(x.OrganisationTypeCode))
            .WithErrorCode(ErrorCodes.MissingPrincipalAddressPostcode);
        RuleFor(x => x.PrincipalAddressPhoneNumber)
            .NotEmpty()
            .When(x => _codes.Contains(x.OrganisationTypeCode))
            .WithErrorCode(ErrorCodes.MissingPrincipalAddressPhoneNumber);
    }
}