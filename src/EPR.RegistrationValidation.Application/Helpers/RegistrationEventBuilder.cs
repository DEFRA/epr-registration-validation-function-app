namespace EPR.RegistrationValidation.Application.Helpers;

using Data.Enums;
using Data.Models;
using Data.Models.SubmissionApi;

public static class RegistrationEventBuilder
{
    public static RegistrationEvent BuildRegistrationEvent(IList<CsvDataRow> csvItems, List<string>? errors, string blobName)
    {
        var requiresBrandsFile = false;
        var requiresPartnershipsFile = false;

        // Goes through each row of the CSV and checks two fields
        // - Packaging Activity SO, this determines whether the brands file is required
        // - Organisation Type Code, this determines whether the partnerships file is required
        foreach (var row in csvItems)
        {
            if (Enum.IsDefined(typeof(RequiredPackagingActivityForBrands), row.PackagingActivitySO))
            {
                requiresBrandsFile = true;
            }

            if (Enum.IsDefined(typeof(RequiredOrganisationTypeCodeForPartners), row.OrganisationTypeCode))
            {
                requiresPartnershipsFile = true;
            }

            if (requiresPartnershipsFile && requiresBrandsFile)
            {
                break;
            }
        }

        return new RegistrationEvent
        {
            Type = EventType.Registration,
            Errors = errors,
            BlobName = blobName,
            ValidationErrors = new List<RegistrationValidationError>(),
            RequiresBrandsFile = requiresBrandsFile,
            RequiresPartnershipsFile = requiresPartnershipsFile,
            IsValid = true,
        };
    }

    public static RegistrationEvent BuildErrorRegistrationEvent(List<string>? errors, string blobName)
    {
        return new RegistrationEvent
        {
            Type = EventType.Registration,
            BlobName = blobName,
            Errors = errors,
            ValidationErrors = new List<RegistrationValidationError>(),
            IsValid = false,
        };
    }
}