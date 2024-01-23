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
            IsValid = !errorCodes.Any(),
        };
    }

    public static ValidationEvent CreateValidationEvent(
        List<OrganisationDataRow> csvItems,
        List<RegistrationValidationError>? validationErrors,
        string blobName,
        string blobContainerName,
        int errorLimit)
    {
        return BuildRegistrationValidationEvent(csvItems, validationErrors: validationErrors, blobName, blobContainerName, errorLimit);
    }

    private static ValidationEvent BuildRegistrationValidationEvent(
        List<OrganisationDataRow> csvItems,
        List<RegistrationValidationError>? validationErrors,
        string blobName,
        string blobContainerName,
        int errorLimit)
    {
        bool requiresPartnershipsFile = csvItems.Exists(row => Enum.IsDefined(typeof(RequiredOrganisationTypeCodeForPartners), row.OrganisationTypeCode));
        bool requiresBrandsFile = csvItems.Exists(row => Enum.IsDefined(typeof(RequiredPackagingActivityForBrands), row.PackagingActivitySO));

        var validationEvent = new RegistrationValidationEvent
        {
            Type = EventType.Registration,
            ValidationErrors = validationErrors,
            RequiresBrandsFile = requiresBrandsFile,
            RequiresPartnershipsFile = requiresPartnershipsFile,
            IsValid = validationErrors?.Count == 0,
            BlobName = blobName,
            BlobContainerName = blobContainerName,
        };

        validationEvent.HasMaxRowErrors = validationEvent.RowErrorCount == errorLimit;

        return validationEvent;
    }
}