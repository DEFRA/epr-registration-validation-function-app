namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class StatusCodeValidatorTests
{
    [TestMethod]
    public async Task Validate_WithSubsidiaryIdPresentAndLeaverDatePresentEmptyStatusCode_IsNotValid()
    {
        // Arrange
        var validator = new StatusCodeValidator(false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverDate = "22/01/2025" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.StatusCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.StatusCodeMustBePresentWhenLeaverDatePresent);
    }

    [TestMethod]
    public async Task Validate_WithUploadedByCSAndLeaverDatePresentEmptyStatusCode_IsNotValid()
    {
        // Arrange
        var validator = new StatusCodeValidator(true);
        var orgDataRow = new OrganisationDataRow { LeaverDate = "22/01/2025" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.StatusCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.StatusCodeMustBePresentWhenLeaverDatePresentCS);
    }

    [TestMethod]
    public async Task Validate_WithInvalidStatusCode_IsNotValid()
    {
        // Arrange
        var validator = new StatusCodeValidator(false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", StatusCode = "a" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.StatusCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidStatusCode);
    }

    [TestMethod]
    public async Task Validate_WithValidStatusCode_IsValid()
    {
        // Arrange
        var validator = new StatusCodeValidator(false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", StatusCode = "A" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
