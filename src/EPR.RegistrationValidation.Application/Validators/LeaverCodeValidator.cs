namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class LeaverCodeValidator : AbstractValidator<OrganisationDataRow>
{
    public LeaverCodeValidator(bool uploadedByComplianceScheme, bool enableLeaverCodeValidation)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        if (enableLeaverCodeValidation)
        {
            this.CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(r => r.LeaverCode)
             .NotEmpty()
             .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverDate))
             .WithErrorCode(ErrorCodes.LeaveOrJoinerCodeShouldNotbeEmpty);

            RuleFor(r => r.LeaverCode)
             .Must(x => AllCodeIsValid(x))
             .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverCode))
             .WithErrorCode(ErrorCodes.InvalidLeaverOrJoinerCode);

            RuleFor(r => r.LeaverCode)
             .NotEmpty()
             .When(x => CodeAndDateAreValid(x.LeaverCode, x.LeaverDate, x.JoinerDate))
             .WithErrorCode(ErrorCodes.LeaveOrJoinerCodeShouldNotbeEmpty);

            RuleFor(r => r.LeaverCode)
              .MinimumLength(2)
              .When(r => !string.IsNullOrEmpty(r.LeaverCode) && CodeAndDateAreValid(r.LeaverCode, r.LeaverDate, r.JoinerDate))
              .WithMessage(ErrorCodes.JoinerOrLeaverCodeMinLengthNotCorrect);
        }
        else
        {
            RuleFor(r => r.LeaverCode)
              .NotEmpty()
              .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverDate))
              .WithErrorCode(ErrorCodes.StatusCodeMustBePresentWhenLeaverDatePresent);

            RuleFor(r => r.LeaverCode)
                .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.SubsidiaryId) && uploadedByComplianceScheme && !string.IsNullOrEmpty(x.LeaverDate))
                .WithErrorCode(ErrorCodes.StatusCodeMustBePresentWhenLeaverDatePresentCS);

            RuleFor(r => r.LeaverCode)
                .Must(x => StatusCodeIsValid(x))
                .When(r => !string.IsNullOrEmpty(r.LeaverCode))
                .WithErrorCode(ErrorCodes.InvalidStatusCode);
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

    private static bool StatusCodeIsValid(string statusCode)
    {
        return statusCode == StatusCode.A ||
            statusCode == StatusCode.B ||
            statusCode == StatusCode.C ||
            statusCode == StatusCode.D ||
            statusCode == StatusCode.E ||
            statusCode == StatusCode.F ||
            statusCode == StatusCode.G ||
            statusCode == StatusCode.H ||
            statusCode == StatusCode.I ||
            statusCode == StatusCode.J ||
            statusCode == StatusCode.K ||
            statusCode == StatusCode.L ||
            statusCode == StatusCode.M ||
            statusCode == StatusCode.N ||
            statusCode == StatusCode.O ||
            statusCode == StatusCode.P ||
            statusCode == StatusCode.Q;
    }
}