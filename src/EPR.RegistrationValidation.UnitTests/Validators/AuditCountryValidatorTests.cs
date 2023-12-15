namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class AuditCountryValidatorTests
{
    [TestMethod]
    public async Task Validate_WithEmptyAuditAddress_IsValid()
    {
        // Arrange
        var validator = new AuditCountryValidator();
        var orgDataRow = new OrganisationDataRow
        {
            AuditAddressCountry = string.Empty,
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
        var validator = new AuditCountryValidator();
        var orgDataRow = new OrganisationDataRow
        {
            AuditAddressCountry = AuditingCountryCodes.England,
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
        var validator = new AuditCountryValidator();
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
        var validator = new AuditCountryValidator();
        var orgDataRow = new OrganisationDataRow
        {
            AuditAddressCountry = AuditingCountryCodes.England.ToLower(),
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}