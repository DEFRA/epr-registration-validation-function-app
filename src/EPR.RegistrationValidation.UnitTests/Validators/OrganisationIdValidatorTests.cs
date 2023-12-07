namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class OrganisationIdValidatorTests
{
    [TestMethod]
    public async Task Validate_WithNullOrganisationId_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow();

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.DefraId);
    }

    [TestMethod]
    public async Task Validate_WithOrganisationId_IsValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private static OrganisationIdValidator CreateValidator()
    {
        return new OrganisationIdValidator();
    }
}