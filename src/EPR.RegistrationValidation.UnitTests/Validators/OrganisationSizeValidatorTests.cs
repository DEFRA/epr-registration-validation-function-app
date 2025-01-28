namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class OrganisationSizeValidatorTests
{
    [TestMethod]
    public async Task Validate_WithOrganisationSize_IsNull_ReturnError()
    {
        // Arrange
        var validator = CreateOrganisationSizeValidator();
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", OrganisationSize = null };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithOrganisationSize_IsInValid_ReturnError()
    {
        // Arrange
        var validator = CreateOrganisationSizeValidator();
        var invalidSizeValue = "x";
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", OrganisationSize = invalidSizeValue };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithOrganisationSize_IsValid_ReturnTrue()
    {
        // Arrange
        var validator = CreateOrganisationSizeValidator();
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", OrganisationSize = "s" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private static OrganisationSizeValidator CreateOrganisationSizeValidator()
    {
        return new OrganisationSizeValidator();
    }
}