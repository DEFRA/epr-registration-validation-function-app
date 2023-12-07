namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PrimaryContactValidatorTests
{
    [TestMethod]
    public async Task Validate_WithNullPrimaryContactFirstName_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow
        {
            PrimaryContactPersonLastName = "Last Name",
            PrimaryContactPersonEmail = "contatemail@test.com",
            PrimaryContactPersonPhoneNumber = "0123765375",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PrimaryContactPersonFirstName);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPrimaryContactFirstName);
    }

    [TestMethod]
    public async Task Validate_WithNullPrimaryContactLastName_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow
        {
            PrimaryContactPersonFirstName = "First Name",
            PrimaryContactPersonEmail = "contatemail@test.com",
            PrimaryContactPersonPhoneNumber = "0123765375",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PrimaryContactPersonLastName);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPrimaryContactLastName);
    }

    [TestMethod]
    public async Task Validate_WithNullPrimaryContactPhoneNumber_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow
        {
            PrimaryContactPersonFirstName = "First Name",
            PrimaryContactPersonLastName = "Last Name",
            PrimaryContactPersonEmail = "contatemail@test.com",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PrimaryContactPersonPhoneNumber);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPrimaryContactPhoneNumber);
    }

    [TestMethod]
    public async Task Validate_WithNullPrimaryContactEmail_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow
        {
            PrimaryContactPersonFirstName = "First Name",
            PrimaryContactPersonLastName = "Last Name",
            PrimaryContactPersonPhoneNumber = "0123765375",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PrimaryContactPersonEmail);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPrimaryContactEmail);
    }

    [TestMethod]
    public async Task Validate_WithPrimaryContactDetails_IsValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "1234567890",
            PrimaryContactPersonFirstName = "First Name",
            PrimaryContactPersonLastName = "Last Name",
            PrimaryContactPersonPhoneNumber = "0123765375",
            PrimaryContactPersonEmail = "name@test.com",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingPrimaryContactFirstName);
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingPrimaryContactLastName);
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingPrimaryContactPhoneNumber);
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingPrimaryContactEmail);
    }

    private static PrimaryContactValidator CreateValidator()
    {
        return new PrimaryContactValidator();
    }
}