namespace EPR.RegistrationValidation.UnitTests.TestHelpers;

using Data.Enums;
using Data.Models;
using Data.Models.SubmissionApi;
using FluentAssertions;

public static class RegistrationEventTestHelper
{
    public static RegistrationEvent BuildRegistrationEvent(
        IList<OrganisationDataRow> csvItems,
        string submissionId,
        string userId,
        string organisationId,
        List<RegistrationValidationError>? validationErrors,
        bool isValid,
        List<string>? errors)
    {
        var requiresBrandsFile = false;
        var requiresPartnershipsFile = false;
        foreach (var row in csvItems)
        {
            if (Enum.IsDefined(typeof(RequiredPackagingActivityForBrands), row.PackagingActivitySO))
            {
                requiresPartnershipsFile = true;
            }

            if (Enum.IsDefined(typeof(RequiredOrganisationTypeCodeForPartners), row.OrganisationTypeCode))
            {
                requiresBrandsFile = true;
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
            ValidationErrors = new List<RegistrationValidationError>(),
            RequiresBrandsFile = requiresBrandsFile,
            RequiresPartnershipsFile = requiresPartnershipsFile,
        };
    }

    public static void AssertRegEvent(
        RegistrationEvent regEvent,
        List<string> errors,
        List<RegistrationValidationError> validationErrors,
        bool hasBrands,
        bool hasPartnerships,
        string blobName)
    {
        regEvent.Errors.Count.Should().Be(errors.Count);
        regEvent.ValidationErrors.Count.Should().Be(validationErrors.Count);
        regEvent.RequiresBrandsFile.Should().Be(hasBrands);
        regEvent.RequiresPartnershipsFile.Should().Be(hasPartnerships);
        regEvent.Type.Should().Be(EventType.Registration);
        regEvent.BlobName.Should().Be(blobName);
    }
}