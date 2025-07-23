namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using FluentValidation;

public class LeaverCodeValidator : AbstractCodeValidator
{
    public LeaverCodeValidator(bool uploadedByComplianceScheme, bool enableLeaverCodeValidation)
        : base(uploadedByComplianceScheme, enableLeaverCodeValidation)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        if (enableLeaverCodeValidation)
        {
            this.CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(r => r.LeaverCode)
             .NotEmpty()
             .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverDate))
             .WithErrorCode(ErrorCodes.LeaveOrJoinderCodeShouldNotbeEmpty);

            RuleFor(r => r.LeaverCode)
             .Must(x => AllCodeIsValid(x))
             .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverCode))
             .WithErrorCode(ErrorCodes.InvalidLeaverOrJoinerCode);

            RuleFor(r => r.LeaverCode)
             .NotEmpty()
             .When(x => CodeAndDateAreValid(x.LeaverCode, x.LeaverDate, x.JoinerDate))
             .WithErrorCode(ErrorCodes.LeaveOrJoinderCodeShouldNotbeEmpty);

            RuleFor(r => r.LeaverCode)
              .Must(x => LeaverCodeIsValid(x))
              .When(x => !string.IsNullOrEmpty(x.LeaverCode) && !string.IsNullOrEmpty(x.LeaverDate) && string.IsNullOrEmpty(x.JoinerDate))
              .WithErrorCode(ErrorCodes.InvalidLeaverOrJoinerCode);

            RuleFor(r => r.LeaverCode)
              .MinimumLength(2)
              .When(r => !string.IsNullOrEmpty(r.LeaverCode) && CodeAndDateAreValid(r.LeaverCode, r.LeaverDate, r.JoinerDate))
              .WithMessage(ErrorCodes.JoinerOrLeaverCodeMinLengthNotCorrect);
        }
    }

    private static bool CodeAndDateAreValid(string leaverORJoinerCode, string leaverDate, string joinerDate)
    {
        bool isItIsLeaverCode = leaverORJoinerCode == LeaverCode.LeaverCode04 ||
            leaverORJoinerCode == LeaverCode.LeaverCode05 ||
            leaverORJoinerCode == LeaverCode.LeaverCode06 ||
            leaverORJoinerCode == LeaverCode.LeaverCode08 ||
            leaverORJoinerCode == LeaverCode.LeaverCode10 ||
            leaverORJoinerCode == LeaverCode.LeaverCode11 ||
            leaverORJoinerCode == LeaverCode.LeaverCode12 ||
            leaverORJoinerCode == LeaverCode.LeaverCode13 ||
            leaverORJoinerCode == LeaverCode.LeaverCode14 ||
            leaverORJoinerCode == LeaverCode.LeaverCode16;

        bool isItIsJoinerCode = leaverORJoinerCode == JoinerCode.LeaverCode01 ||
            leaverORJoinerCode == JoinerCode.LeaverCode02 ||
            leaverORJoinerCode == JoinerCode.LeaverCode03 ||
            leaverORJoinerCode == JoinerCode.LeaverCode07 ||
            leaverORJoinerCode == JoinerCode.LeaverCode09 ||
            leaverORJoinerCode == JoinerCode.LeaverCode15 ||
            leaverORJoinerCode == JoinerCode.LeaverCode17;

        if (isItIsLeaverCode && !string.IsNullOrEmpty(leaverDate))
        {
            return true;
        }
        else if (isItIsJoinerCode && !string.IsNullOrEmpty(joinerDate))
        {
            return true;
        }

        return false;
    }

    private static bool LeaverCodeIsValid(string leaverCode)
    {
        return leaverCode == LeaverCode.LeaverCode04 ||
            leaverCode == LeaverCode.LeaverCode05 ||
            leaverCode == LeaverCode.LeaverCode06 ||
            leaverCode == LeaverCode.LeaverCode08 ||
            leaverCode == LeaverCode.LeaverCode10 ||
            leaverCode == LeaverCode.LeaverCode11 ||
            leaverCode == LeaverCode.LeaverCode12 ||
            leaverCode == LeaverCode.LeaverCode13 ||
            leaverCode == LeaverCode.LeaverCode14 ||
            leaverCode == LeaverCode.LeaverCode16;
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