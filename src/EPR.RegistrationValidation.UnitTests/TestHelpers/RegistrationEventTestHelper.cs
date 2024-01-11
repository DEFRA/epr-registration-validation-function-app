namespace EPR.RegistrationValidation.UnitTests.TestHelpers;

using Data.Enums;
using Data.Models;
using Data.Models.SubmissionApi;
using FluentAssertions;

public static class RegistrationEventTestHelper
{
    public static ValidationEvent BuildRegistrationEvent(
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

        return new RegistrationValidationEvent
        {
            Type = EventType.Registration,
            Errors = errors,
            ValidationErrors = new List<RegistrationValidationError>(),
            RequiresBrandsFile = requiresBrandsFile,
            RequiresPartnershipsFile = requiresPartnershipsFile,
        };
    }

    public static void AssertRegistrationValidationEvent(
        ValidationEvent regEvent,
        List<RegistrationValidationError> validationErrors,
        bool hasBrands,
        bool hasPartnerships,
        string blobName)
    {
        regEvent.Should().BeOfType<RegistrationValidationEvent>();
        var registrationEvent = (RegistrationValidationEvent)regEvent;
        registrationEvent.ValidationErrors.Count.Should().Be(validationErrors.Count);
        registrationEvent.RequiresBrandsFile.Should().Be(hasBrands);
        registrationEvent.RequiresPartnershipsFile.Should().Be(hasPartnerships);
        registrationEvent.Type.Should().Be(EventType.Registration);
        registrationEvent.BlobName.Should().Be(blobName);
    }

    public static void AssertValidationEvent(
        ValidationEvent regEvent,
        EventType eventType,
        string blobName,
        string blobContainerName,
        params string[] errors)
    {
        regEvent.Type.Should().Be(eventType);
        regEvent.BlobName.Should().Be(blobName);
        regEvent.BlobContainerName.Should().Be(blobContainerName);
        regEvent.Errors.Count.Should().Be(errors.Length);
    }
}