namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class JoinerCodeValidatorTests
{
    [TestMethod]
    public async Task Validate_WithSubsidiaryIdPresentAndLeaverDatePresentEmptyStatusCode_IsNotValid()
    {
        // Arrange
        var validator = new LeaverCodeValidator(false, false);
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
    public async Task Validate_WithUploadedByCSAndLeaverDatePresentEmptyStatusCode_IsNotValid()
    {
        // Arrange
        var validator = new LeaverCodeValidator(true, false);
        var orgDataRow = new OrganisationDataRow { LeaverDate = "22/01/2025" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.StatusCodeMustBePresentWhenLeaverDatePresentCS);
    }

    [TestMethod]
    public async Task Validate_WithInvalidStatusCode_IsNotValid()
    {
        // Arrange
        var validator = new LeaverCodeValidator(false, false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "a" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidStatusCode);
    }

    [TestMethod]
    [DataRow("A")]
    [DataRow("B")]
    [DataRow("C")]
    public async Task Validate_WithInvalidLeaverCode_IsNotValid(string leaverCode)
    {
        // Arrange
        var validator = new LeaverCodeValidator(false, true);
        var orgDataRow = new OrganisationDataRow
        {
            LeaverCode = leaverCode,
            SubsidiaryId = "1",
            JoinerDate = "01/01/2000",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidLeaverOrJoinerCode);
    }

    [TestMethod]
    [DataRow("ABC")]
    [DataRow("01-01-2000")]
    [DataRow("01.01.2000")]
    public async Task Validate_WithInvalidLeaverCode_IsNotValid2(string leaverCode)
    {
        // Arrange
        var validator = new LeaverCodeValidator(false, true);

        var orgDataRow = new OrganisationDataRow
        {
            LeaverCode = leaverCode,
            SubsidiaryId = "1",
            JoinerDate = "01/01/2000",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidLeaverOrJoinerCode);
    }

    [TestMethod]
    [DataRow("01")]
    [DataRow("02")]
    [DataRow("03")]
    [DataRow("10")]
    [DataRow("12")]
    [DataRow("13")]
    [DataRow("18")]
    [DataRow("19")]
    [DataRow("20")]
    [DataRow("21")]
    public async Task Validate_WithValidLeaverCode_IsValid(string leaverCode)
    {
        // Arrange
        var validator = new LeaverCodeValidator(false, true);
        var orgDataRow = new OrganisationDataRow
        {
            LeaverCode = leaverCode,
            SubsidiaryId = "1",
            JoinerDate = "01/01/2000",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("01")]
    public async Task Validate_With_ValidLeaverCode_IsValid(string leaverCode)
    {
        // Arrange
        var validator = new LeaverCodeValidator(false, true);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = leaverCode };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("1")]
    [DataRow("2")]
    [DataRow("3")]
    public async Task Validate_With_InValidJoinerCode_InValid(string joinerCode)
    {
        // Arrange
        var validator = new LeaverCodeValidator(false, true);
        var orgDataRow = new OrganisationDataRow
        {
            LeaverCode = joinerCode,
            SubsidiaryId = "1",
            JoinerDate = "01/01/2000",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidLeaverOrJoinerCode);
    }

    [TestMethod]
    public async Task Validate_WithValidStatusCode_IsValid()
    {
        // Arrange
        var validator = new LeaverCodeValidator(false, false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "A" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
