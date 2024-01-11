namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using EPR.RegistrationValidation.Application.Constants;
using Extensions;
using FluentValidation;

public class PartnerDataRowCharacterLengthValidator : AbstractValidator<PartnersDataRow>
{
    public PartnerDataRowCharacterLengthValidator()
    {
        RuleFor(x => x.DefraId)
            .Length(0, CharacterLimits.MaxLength)
            .WithErrorCode(ErrorCodes.CharacterLengthExceeded);
        RuleFor(x => x.SubsidiaryId)
            .Length(0, CharacterLimits.MaxLength)
            .WithErrorCode(ErrorCodes.CharacterLengthExceeded);
        RuleFor(x => x.PartnerFirstName)
            .Length(0, CharacterLimits.MaxLength)
            .WithErrorCode(ErrorCodes.CharacterLengthExceeded);
        RuleFor(x => x.PartnerLastName)
            .Length(0, CharacterLimits.MaxLength)
            .WithErrorCode(ErrorCodes.CharacterLengthExceeded);
        RuleFor(x => x.PartnerPhoneNumber)
            .Length(0, CharacterLimits.MaxLength)
            .WithErrorCode(ErrorCodes.CharacterLengthExceeded);
        RuleFor(x => x.PartnerEmail)
            .Length(0, CharacterLimits.MaxLength)
            .WithErrorCode(ErrorCodes.CharacterLengthExceeded);
    }
}