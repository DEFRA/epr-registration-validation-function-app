namespace EPR.RegistrationValidation.Data.UnitTests.Models.SubmissionApi;

using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Enums;
using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.Data.Models.SubmissionApi;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class RegistrationEventTests
{
    [TestMethod]
    public void RegistrationValidationEvent_PropertyValidation_ReturnsExpectedResults()
    {
        // Arrange
        var registrationEvent = new RegistrationValidationEvent
        {
            Type = EventType.Registration,
            ValidationErrors = new List<RegistrationValidationError>
            {
                new()
                {
                    ColumnErrors = new List<ColumnValidationError>
                    {
                        new()
                        {
                            ColumnIndex = 0,
                            ErrorCode = ErrorCodes.FileFormatInvalid,
                            ColumnName = "column_name",
                        },
                    },
                    RowNumber = 1,
                },
            },
            RequiresBrandsFile = true,
            RequiresPartnershipsFile = false,
        };

        // Act
        var eventType = registrationEvent.Type;
        var validationErrors = registrationEvent.ValidationErrors;
        var requiresBrandsFile = registrationEvent.RequiresBrandsFile;
        var requiresPartnershipsFile = registrationEvent.RequiresPartnershipsFile;

        // Assert
        eventType.Should().Be(EventType.Registration);
        validationErrors.Should().HaveCount(1);
        requiresBrandsFile.Should().BeTrue();
        requiresPartnershipsFile.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(EventType.BrandValidation)]
    [DataRow(EventType.PartnerValidation)]
    public void ValidationEvent_PropertyValidation_ReturnsExpectedResults(EventType eventTypeValue)
    {
        // Arrange
        var errorCode = "99";

        var registrationEvent = new ValidationEvent
        {
            Type = eventTypeValue,
            Errors = new List<string>
            {
                errorCode,
            },
            IsValid = false,
            BlobName = "blobName",
            BlobContainerName = "blobContainerName",
        };

        // Act
        var eventType = registrationEvent.Type;
        var errors = registrationEvent.Errors;
        var isValid = registrationEvent.IsValid;
        var blobName = registrationEvent.BlobName;
        var blobContainerName = registrationEvent.BlobContainerName;

        // Assert
        eventType.Should().Be(eventTypeValue);
        errors.Should().HaveCount(1);
        errors.Should().ContainSingle(errorCode);
        isValid.Should().Be(isValid);
        blobName.Should().Be(blobName);
        blobContainerName.Should().Be(blobContainerName);
    }
}