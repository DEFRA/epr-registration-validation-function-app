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
        var validator = CreateOrganisationSizeValidator(false, false);
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
        var validator = CreateOrganisationSizeValidator(false, false);
        var invalidSizeValue = "x";
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", OrganisationSize = invalidSizeValue };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [TestMethod]
    public async Task ValidateDP_WhenOrganisationSizeIsSmall_ReturnCorrectResultBasedOnRegDate()
    {
        // Arrange
        var validator = CreateOrganisationSizeValidator(false, true);
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", OrganisationSize = "s" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        // Small producers registration window for 2026
        if (DateTime.UtcNow >= new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) && DateTime.UtcNow <= new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Unspecified))
        {
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }
    }

    [TestMethod]
    public async Task ValidateCS_WhenOrganisationSizeIsSmall_ReturnCorrectResultBasedOnRegDate()
    {
        // Arrange
        var validator = CreateOrganisationSizeValidator(true, true);
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", OrganisationSize = "s" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        // Small producers registration window for 2026
        if (DateTime.UtcNow >= new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) && DateTime.UtcNow <= new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Unspecified))
        {
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }
    }

    [TestMethod]
    public async Task ValidateCS_WhenOrganisationSizeIsLarge_ReturnIsValid()
    {
        // Arrange
        var validator = CreateOrganisationSizeValidator(false, true);
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", OrganisationSize = "L" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private static OrganisationSizeValidator CreateOrganisationSizeValidator(bool uploadedByComplianceScheme, bool isSubmissionPeriod2026)
    {
        DateTime smallProducersRegStartTime2026 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime smallProducersRegEndTime2026 = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return new OrganisationSizeValidator(uploadedByComplianceScheme, isSubmissionPeriod2026, smallProducersRegStartTime2026, smallProducersRegEndTime2026);
    }
}