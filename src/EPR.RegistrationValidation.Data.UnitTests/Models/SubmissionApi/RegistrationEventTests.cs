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
    public void RegistrationEvent_PropertyValidation_ReturnsExpectedResults()
    {
        // Arrange
        var errorCode = "99";

        var registrationEvent = new RegistrationEvent
        {
            Type = EventType.Registration,
            Errors = new List<string>
            {
                errorCode,
            },
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
        var errors = registrationEvent.Errors;
        var validationErrors = registrationEvent.ValidationErrors;
        var requiresBrandsFile = registrationEvent.RequiresBrandsFile;
        var requiresPartnershipsFile = registrationEvent.RequiresPartnershipsFile;

        // Assert
        eventType.Should().Be(EventType.Registration);
        errors.Should().HaveCount(1);
        errors.Should().ContainSingle(errorCode);
        validationErrors.Should().HaveCount(1);
        requiresBrandsFile.Should().BeTrue();
        requiresPartnershipsFile.Should().BeFalse();
    }
}