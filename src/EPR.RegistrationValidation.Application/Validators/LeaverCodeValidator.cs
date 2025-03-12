namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class LeaverCodeValidator : AbstractValidator<OrganisationDataRow>
{
    public LeaverCodeValidator(bool uploadedByComplianceScheme)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.LeaverCode)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverDate))
            .WithErrorCode(ErrorCodes.LeaverCodeMustBePresentWhenLeaverDatePresent);

        RuleFor(r => r.LeaverCode)
            .NotEmpty()
            .When(x => string.IsNullOrEmpty(x.SubsidiaryId) && uploadedByComplianceScheme && !string.IsNullOrEmpty(x.LeaverDate))
            .WithErrorCode(ErrorCodes.LeaverCodeMustBePresentWhenLeaverDatePresentCS);

        RuleFor(r => r.LeaverCode)
            .Must(x => LeaverCodeIsValid(x))
            .When(r => !string.IsNullOrEmpty(r.LeaverCode))
            .WithErrorCode(ErrorCodes.InvalidLeaverCode);
    }

    private bool LeaverCodeIsValid(string leaverCode)
    {
        return leaverCode == LeaverCode.Administration ||
            leaverCode == LeaverCode.Liquidation ||
            leaverCode == LeaverCode.DroppedBelowTurnoverThreshold ||
            leaverCode == LeaverCode.DroppedBelowTonnageThreshold ||
            leaverCode == LeaverCode.Resignation ||
            leaverCode == LeaverCode.SchemeHasTerminatedMembership ||
            leaverCode == LeaverCode.BusinessClosure ||
            leaverCode == LeaverCode.Bankruptcy ||
            leaverCode == LeaverCode.MergedWithAnotherCompany ||
            leaverCode == LeaverCode.SubsidiaryOfAnotherCompany ||
            leaverCode == LeaverCode.NotReadyToRegisterByApril ||
            leaverCode == LeaverCode.NoLongerObligated;
    }
}
