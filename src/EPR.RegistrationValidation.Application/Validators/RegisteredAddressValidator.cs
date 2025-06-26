namespace EPR.RegistrationValidation.Application.Validators;

using Constants;
using Data.Constants;
using Data.Models;
using FluentValidation;

public class RegisteredAddressValidator : AbstractValidator<OrganisationDataRow>
{
    private static readonly string[] _validCodes =
    {
        IncorporationTypeCodes.LimitedPartnership,
        IncorporationTypeCodes.LimitedLiabilityPartnership,
        IncorporationTypeCodes.LimitedCompany,
        IncorporationTypeCodes.PublicLimitedCompany,
        HybridCorporationType.CommunityInterestCompany,
    };

    public RegisteredAddressValidator()
    {
        RuleFor(x => x.RegisteredAddressLine1)
            .NotEmpty()
            .When(x => _validCodes.Contains(x.OrganisationTypeCode))
            .WithErrorCode(ErrorCodes.MissingRegisteredAddressLine1);
        RuleFor(x => x.RegisteredAddressPostcode)
            .NotEmpty()
            .When(x => _validCodes.Contains(x.OrganisationTypeCode))
            .WithErrorCode(ErrorCodes.MissingRegisteredAddressPostcode);
        RuleFor(x => x.RegisteredAddressPhoneNumber)
            .NotEmpty()
            .When(x => _validCodes.Contains(x.OrganisationTypeCode))
            .WithErrorCode(ErrorCodes.MissingRegisteredAddressPhoneNumber);
    }
}