namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class AuditAddressValidatorTests
{
    [TestMethod]
    public async Task Validate_WithEmptyAuditAddress_IsValid()
    {
        // Arrange
        var validator = new AuditAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            AuditAddressLine1 = string.Empty,
            AuditAddressCountry = string.Empty,
            AuditAddressPostcode = string.Empty,
            AuditAddressCity = string.Empty,
            AuditAddressCounty = string.Empty,
            AuditAddressLine2 = string.Empty,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithValidAuditCountry_IsValid()
    {
        // Arrange
        var validator = new AuditAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            AuditAddressLine1 = "10 Southcote",
            AuditAddressCountry = AuditingCountryCodes.England,
            AuditAddressPostcode = "KT5 9UW",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithInvalidAuditCountry_IsNotValid()
    {
        // Arrange
        var validator = new AuditAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            AuditAddressCountry = "invalidCode",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.AuditAddressCountry);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidAuditAddressCountry);
    }

    [TestMethod]
    public async Task Validate_WithValidAuditCountryInLowerCase_IsValid()
    {
        // Arrange
        var validator = new AuditAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            AuditAddressLine1 = "10 Southcote",
            AuditAddressCountry = AuditingCountryCodes.England.ToLower(),
            AuditAddressPostcode = "KT5 9UW",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_AuditAddress_WithEmptyAddressLine1_IsNotValid()
    {
        // Arrange
        var validator = new AuditAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
           AuditAddressLine1 = null,
           AuditAddressPostcode = "KT5 9UW",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.AuditAddressLine1);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingAuditAddressLine1);
    }

    [TestMethod]
    public async Task Validate_AuditAddress_WithEmptyPostcode_IsNotValid()
    {
        // Arrange
        var validator = new AuditAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            AuditAddressLine1 = "10 Southcote",
            AuditAddressPostcode = null,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.AuditAddressPostcode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingAuditPostcode);
    }

    [TestMethod]
    public async Task Validate_AuditAddress_WithoutAnyRequiredFields_IsNotValid()
    {
        // Arrange
        var validator = new AuditAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            AuditAddressLine1 = null,
            AuditAddressPostcode = null,
            AuditAddressCountry = AuditingCountryCodes.England.ToLower(),
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.AuditAddressLine1);
        result.ShouldHaveValidationErrorFor(x => x.AuditAddressPostcode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingAuditAddressLine1);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingAuditPostcode);
    }
}