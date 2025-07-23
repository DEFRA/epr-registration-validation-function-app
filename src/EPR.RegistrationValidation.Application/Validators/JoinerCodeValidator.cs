namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using FluentValidation;

public class JoinerCodeValidator : AbstractCodeValidator
{
    public JoinerCodeValidator(bool uploadedByComplianceScheme, bool enableLeaverCodeValidation)
        : base(uploadedByComplianceScheme, enableLeaverCodeValidation)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        if (enableLeaverCodeValidation)
        {
            RuleFor(r => r.LeaverCode)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverDate))
            .WithErrorCode(ErrorCodes.StatusCodeMustBePresentWhenLeaverDatePresent);

            RuleFor(r => r.LeaverCode)
            .Must(x => AllCodeIsValid(x))
            .WithErrorCode(ErrorCodes.InvalidLeaverOrJoinerCode);

            RuleFor(r => r.LeaverCode)
            .Must(x => JoinerCodeIsValid(x))
            .When(x => !string.IsNullOrEmpty(x.LeaverDate))
            .WithErrorCode(ErrorCodes.LeaverDateShouldNotBePresent);
        }
    }

    private static bool JoinerCodeIsValid(string joinerCode)
    {
        return joinerCode == JoinerCode.LeaverCode01 ||
            joinerCode == JoinerCode.LeaverCode02 ||
            joinerCode == JoinerCode.LeaverCode03 ||
            joinerCode == JoinerCode.LeaverCode07 ||
            joinerCode == JoinerCode.LeaverCode09 ||
            joinerCode == JoinerCode.LeaverCode15 ||
            joinerCode == JoinerCode.LeaverCode17;
    }

    private static bool AllCodeIsValid(string code)
    {
        return code == LeaverCode.LeaverCode04 ||
          code == LeaverCode.LeaverCode05 ||
          code == LeaverCode.LeaverCode06 ||
          code == LeaverCode.LeaverCode08 ||
          code == LeaverCode.LeaverCode10 ||
          code == LeaverCode.LeaverCode11 ||
          code == LeaverCode.LeaverCode12 ||
          code == LeaverCode.LeaverCode13 ||
          code == LeaverCode.LeaverCode14 ||
          code == LeaverCode.LeaverCode16 ||
          code == JoinerCode.LeaverCode01 ||
          code == JoinerCode.LeaverCode02 ||
          code == JoinerCode.LeaverCode03 ||
          code == JoinerCode.LeaverCode07 ||
          code == JoinerCode.LeaverCode09 ||
          code == JoinerCode.LeaverCode15 ||
          code == JoinerCode.LeaverCode17;
    }
}
