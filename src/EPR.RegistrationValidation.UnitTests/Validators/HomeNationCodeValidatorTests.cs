namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class HomeNationCodeValidatorTests
{
    [TestMethod]
    public async Task Validate_WithNullHomeNationCode_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow();

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.HomeNationCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingHomeNationCode);
    }

    [TestMethod]
    public async Task Validate_WithValidHomeNationCode_IsValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", HomeNationCode = "EN" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithInvalidHomeNationCode_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", HomeNationCode = "home" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.HomeNationCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidHomeNationCode);
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.MissingHomeNationCode);
    }

    private static HomeNationCodeValidator CreateValidator()
    {
        return new HomeNationCodeValidator();
    }
}