namespace EPR.RegistrationValidation.Application.Validators;

using Data.Constants;
using Data.Models;
using EPR.RegistrationValidation.Application.Constants;
using FluentValidation;

public class BrandDataRowCharacterLengthValidator : AbstractValidator<BrandDataRow>
{
    public BrandDataRowCharacterLengthValidator()
    {
        RuleFor(x => x.DefraId)
            .MaximumLength(CharacterLimits.MaxLength)
            .WithErrorCode(ErrorCodes.CharacterLengthExceeded);
        RuleFor(x => x.SubsidiaryId)
            .MaximumLength(CharacterLimits.MaxLength)
            .WithErrorCode(ErrorCodes.CharacterLengthExceeded);
        RuleFor(x => x.BrandName)
            .MaximumLength(CharacterLimits.MaxLength)
            .WithErrorCode(ErrorCodes.CharacterLengthExceeded);
        RuleFor(x => x.BrandTypeCode)
            .MaximumLength(CharacterLimits.MaxLength)
            .WithErrorCode(ErrorCodes.CharacterLengthExceeded);
    }
}