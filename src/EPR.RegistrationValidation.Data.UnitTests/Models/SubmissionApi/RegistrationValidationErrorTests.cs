namespace EPR.RegistrationValidation.Data.UnitTests.Models.SubmissionApi;

using Constants;
using Data.Models.SubmissionApi;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class RegistrationValidationErrorTests
{
    [TestMethod]
    public void RegistrationValidationError_PropertyValidation_ReturnsExpectedResults()
    {
        // Arrange
        var registrationValidationError = new RegistrationValidationError
        {
            RowNumber = 1,
            ErrorCode = new List<string> { ErrorCodes.FileFormatInvalid },
        };

        // Act
        var rowNumber = registrationValidationError.RowNumber;
        var errorCodes = registrationValidationError.ErrorCode;

        // Assert
        rowNumber.Should().Be(1);
        errorCodes.Should().Contain(ErrorCodes.FileFormatInvalid);
    }
}