namespace EPR.RegistrationValidation.Application.Helpers;

using Data.Enums;
using Data.Models;
using Data.Models.SubmissionApi;

public static class RegistrationEventBuilder
{
    public static ValidationEvent CreateValidationEvent(
        EventType eventType,
        string blobName,
        string blobContainerName,
        params string[] errorCodes)
    {
        return new ValidationEvent
        {
            Type = eventType,
            BlobName = blobName,
            BlobContainerName = blobContainerName,
            Errors = errorCodes.ToList(),
            IsValid = errorCodes.Length == 0,
        };
    }

    public static ValidationEvent CreateValidationEvent(
        List<OrganisationDataRow> csvItems,
        List<RegistrationValidationError>? validationErrors,
        List<RegistrationValidationWarning>? validationWarnings,
        string blobName,
        string blobContainerName,
        int errorLimit,
        int? organisationMemberCount)
    {
        return BuildRegistrationValidationEvent(csvItems, validationErrors: validationErrors, validationWarnings: validationWarnings,  blobName, blobContainerName, errorLimit, organisationMemberCount);
    }

    private static RegistrationValidationEvent BuildRegistrationValidationEvent(
        List<OrganisationDataRow> csvItems,
        List<RegistrationValidationError>? validationErrors,
        List<RegistrationValidationWarning>? validationWarnings,
        string blobName,
        string blobContainerName,
        int errorLimit,
        int? organisationMemberCount)
    {
        bool requiresPartnershipsFile = csvItems.Exists(row => Enum.TryParse(typeof(RequiredOrganisationTypeCodeForPartners), row.OrganisationTypeCode, true, out _));
        bool requiresBrandsFile = csvItems.Exists(row => Enum.TryParse(typeof(RequiredPackagingActivityForBrands), row.PackagingActivitySO, true, out _));

        var validationEvent = new RegistrationValidationEvent
        {
            Type = EventType.Registration,
            ValidationErrors = validationErrors,
            ValidationWarnings = validationWarnings,
            RequiresBrandsFile = requiresBrandsFile,
            RequiresPartnershipsFile = requiresPartnershipsFile,
            IsValid = validationErrors?.Count == 0,
            BlobName = blobName,
            BlobContainerName = blobContainerName,
            OrganisationMemberCount = organisationMemberCount,
        };

        validationEvent.HasMaxRowErrors = validationEvent.RowErrorCount == errorLimit;
        validationEvent.HasMaxRowWarnings = validationEvent.RowWarningCount == errorLimit;
        return validationEvent;
    }
}