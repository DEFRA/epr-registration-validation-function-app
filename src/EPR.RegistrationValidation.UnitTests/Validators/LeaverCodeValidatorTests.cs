namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class LeaverCodeValidatorTests
{
    [TestMethod]
    public async Task Validate_WithSubsidiaryIdPresentAndLeaverDatePresentEmptyLeaverCode_IsNotValid()
    {
        // Arrange
        var validator = new LeaverCodeValidator(false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverDate = "22/01/2025" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.LeaverCodeMustBePresentWhenLeaverDatePresent);
    }

    [TestMethod]
    public async Task Validate_WithUploadedByCSAndLeaverDatePresentEmptyLeaverCode_IsNotValid()
    {
        // Arrange
        var validator = new LeaverCodeValidator(true);
        var orgDataRow = new OrganisationDataRow { LeaverDate = "22/01/2025" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.LeaverCodeMustBePresentWhenLeaverDatePresentCS);
    }

    [TestMethod]
    public async Task Validate_WithInvalidLeaverCode_IsNotValid()
    {
        // Arrange
        var validator = new LeaverCodeValidator(false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "a" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidLeaverCode);
    }

    [TestMethod]
    public async Task Validate_WithValidLeaverCode_IsValid()
    {
        // Arrange
        var validator = new LeaverCodeValidator(false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "A" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
