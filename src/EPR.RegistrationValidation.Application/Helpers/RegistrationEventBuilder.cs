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
        string blobContainerName)
    {
        return BuildRegistrationValidationEvent(csvItems, validationErrors: validationErrors, blobName, blobContainerName);
    }

    private static ValidationEvent BuildRegistrationValidationEvent(
        List<OrganisationDataRow> csvItems,
        IList<RegistrationValidationError> validationErrors,
        string blobName,
        string blobContainerName)
    {
        bool requiresPartnershipsFile = csvItems.Exists(row => Enum.IsDefined(typeof(RequiredOrganisationTypeCodeForPartners), row.OrganisationTypeCode));
        bool requiresBrandsFile = csvItems.Exists(row => Enum.IsDefined(typeof(RequiredPackagingActivityForBrands), row.PackagingActivitySO));

        return new RegistrationValidationEvent
        {
            Type = EventType.Registration,
            BlobName = blobName,
            BlobContainerName = blobContainerName,
            ValidationErrors = validationErrors.ToList(),
            RequiresBrandsFile = requiresBrandsFile,
            RequiresPartnershipsFile = requiresPartnershipsFile,
            IsValid = validationErrors.Count == 0,
        };
    }
}