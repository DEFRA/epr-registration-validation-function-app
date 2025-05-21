namespace EPR.RegistrationValidation.Application.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentValidation;

public class StatusCodeValidator : AbstractValidator<OrganisationDataRow>
{
    public StatusCodeValidator(bool uploadedByComplianceScheme)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.StatusCode)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.SubsidiaryId) && !string.IsNullOrEmpty(x.LeaverDate))
            .WithErrorCode(ErrorCodes.StatusCodeMustBePresentWhenLeaverDatePresent);

        RuleFor(r => r.StatusCode)
            .NotEmpty()
            .When(x => string.IsNullOrEmpty(x.SubsidiaryId) && uploadedByComplianceScheme && !string.IsNullOrEmpty(x.LeaverDate))
            .WithErrorCode(ErrorCodes.StatusCodeMustBePresentWhenLeaverDatePresentCS);

        RuleFor(r => r.StatusCode)
            .Must(x => StatusCodeIsValid(x))
            .When(r => !string.IsNullOrEmpty(r.StatusCode))
            .WithErrorCode(ErrorCodes.InvalidStatusCode);
    }

    private bool StatusCodeIsValid(string statusCode)
    {
        return statusCode == StatusCode.BusinessIsNowSmallProducer ||
            statusCode == StatusCode.Liquidation ||
            statusCode == StatusCode.ConfirmedMemberAwaitingPayment ||
            statusCode == StatusCode.ConfirmedMemberAwaitingPackagingData ||
            statusCode == StatusCode.ConfirmedMemberAwaitingRegFile ||
            statusCode == StatusCode.ProducerNoLongerObligatedBelowTurnover ||
            statusCode == StatusCode.ProducerNoLongerObligatedBelowPackagingThreshold ||
            statusCode == StatusCode.ProducerNoLongerObligatedNoLongerProducer ||
            statusCode == StatusCode.SmallProducerJoinedGroupButStillReportsForItself ||
            statusCode == StatusCode.SmallProducerJoinedGroupAndParentReportsOnItsBehalf ||
            statusCode == StatusCode.SmallProducerLeavesGroupReportedForItself ||
            statusCode == StatusCode.SmallProducerLeavesGroupParentReportedForIt ||
            statusCode == StatusCode.LargeProducerLeftGroupButStartsToReportForItself ||
            statusCode == StatusCode.LargeProducerLeftGroupThatReportedForIt ||
            statusCode == StatusCode.LargeProducerJoinedGroupAndParentReportsOnItsBehalf;
    }
}
