﻿namespace EPR.RegistrationValidation.UnitTests.Validators;

using System.Threading.Tasks;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class LeaverDateValidatorTests
{
    [TestMethod]
    public async Task Validate_WithSubsidiaryIdAndLeaverCodeNotEmptyAndEmptyLeaverDate_IsNotValid()
    {
        // Arrange
        var validator = new LeaverDateValidator(false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "A" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverDate);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.LeaverDateMustBePresentWhenLeaverCodePresent);
    }

    [TestMethod]
    public async Task Validate_WithUploadedByCSAndLeaverCodeNotEmptyAndEmptyLeaverDate_IsNotValid()
    {
        // Arrange
        var validator = new LeaverDateValidator(true);
        var orgDataRow = new OrganisationDataRow { LeaverCode = "A" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverDate);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.LeaverDateMustBePresentWhenLeaverCodePresentCS);
    }

    [TestMethod]
    [DataRow("2000/01/01")]
    [DataRow("2000/20/01")]
    [DataRow("2000/1/1")]
    [DataRow("01-01-2000")]
    [DataRow("01.01.2000")]
    public async Task Validate_WithSubsidiaryIdAndLeaverCodeNotEmptyAndInvalidLeaverDateFormat_IsNotValid(string date)
    {
        // Arrange
        var validator = new LeaverDateValidator(false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "Any", LeaverDate = date };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverDate);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidLeaverDateFormat);
    }

    [TestMethod]
    public async Task Validate_WithSubsidiaryIdAndLeaverDateBeforeJoinerDate_IsInvalid()
    {
        // Arrange
        var validator = new LeaverDateValidator(false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "Any", JoinerDate = "02/01/2000", LeaverDate = "01/01/2000" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverDate);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.LeaverDateMustBeAfterJoinerDate);
    }

    [TestMethod]
    public async Task Validate_WithSubsidiaryIdAndCSUploadAndLeaverDateBeforeJoinerDate_IsInvalid()
    {
        // Arrange
        var validator = new LeaverDateValidator(true);
        var orgDataRow = new OrganisationDataRow { LeaverCode = "Any", JoinerDate = "02/01/2000", LeaverDate = "01/01/2000" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverDate);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.LeaverDateMustBeAfterJoinerDateCS);
    }

    [TestMethod]
    public async Task Validate_WithSubsidiaryIdAndLeaverDateInTheFuture_IsInvalid()
    {
        // Arrange
        var validator = new LeaverDateValidator(false);
        var orgDataRow = new OrganisationDataRow
        {
            SubsidiaryId = "1",
            LeaverCode = "A",
            JoinerDate = "02/01/2000",
            LeaverDate = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy"),
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverDate);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.LeaverDateCannotBeInTheFuture);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("2000/01/01")]
    [DataRow("2000/20/01")]
    [DataRow("2000/1/1")]
    [DataRow("01-01-2000")]
    [DataRow("01.01.2000")]
    public async Task Validate_WithSubsidiaryIdAndInvalidJoinerDateAndValidLeaverDate_IsValid(string date)
    {
        // Arrange
        var validator = new LeaverDateValidator(false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", JoinerDate = date, LeaverDate = "01/01/2000" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithSubsidiaryIdAndLeaverCodeNotEmptyAndValidLeaverDateFormat_IsValid()
    {
        // Arrange
        var validator = new LeaverDateValidator(false);
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "Any", JoinerDate = "01/01/2000", LeaverDate = "02/01/2000" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
