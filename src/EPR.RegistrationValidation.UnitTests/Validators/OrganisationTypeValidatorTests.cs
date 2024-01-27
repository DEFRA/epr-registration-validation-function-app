namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class OrganisationTypeValidatorTests
{
    [TestMethod]
    public async Task Validate_WithNullOrganisationTypeCode_IsNotValid()
    {
        // Arrange
        var validator = new OrganisationTypeValidator();
        var orgDataRow = new OrganisationDataRow();

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.OrganisationTypeCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingOrganisationTypeCode);
    }

    [TestMethod]
    public async Task Validate_WithIncorrectOrganisationTypeCode_IsNotValid()
    {
        // Arrange
        var validator = new OrganisationTypeValidator();
        var orgDataRow = new OrganisationDataRow
        {
            OrganisationTypeCode = "InvalidCode",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.OrganisationTypeCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidOrganisationTypeCode);
    }

    [TestMethod]
    public async Task Validate_WithCorrectOrganisationTypeCode_IsValid()
    {
        // Arrange
        var validator = new OrganisationTypeValidator();
        var orgDataRow = new OrganisationDataRow
        {
            OrganisationTypeCode = "LLP",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithCorrectOrganisationTypeCodeNonCaseSensitive_IsValid()
    {
        // Arrange
        var validator = new OrganisationTypeValidator();
        var orgDataRow = new OrganisationDataRow
        {
            OrganisationTypeCode = "Cic",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}