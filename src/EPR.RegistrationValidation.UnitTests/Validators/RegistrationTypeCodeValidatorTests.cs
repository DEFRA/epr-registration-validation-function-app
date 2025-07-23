namespace EPR.RegistrationValidation.UnitTests.Validators;

using System.Threading.Tasks;
using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class RegistrationTypeCodeValidatorTests
{
    private RegistrationTypeCodeValidator _validator;

    [TestInitialize]
    public void Setup()
    {
        _validator = new RegistrationTypeCodeValidator(false);
    }

    [TestMethod]
    public async Task Validate_WithEmptyRegistrationTypeCode_And_PresentSubsidiaryId_IsNotValid()
    {
        // Arrange
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", RegistrationTypeCode = string.Empty };

        // Act
        var result = await _validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.RegistrationTypeCode)
            .WithErrorCode(ErrorCodes.RegistrationTypeCodeIsMandatory);
    }

    [TestMethod]
    public async Task Validate_WithEmptyRegistrationTypeCode_And_EmptySubsidiaryId_IsValid()
    {
        // Arrange
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = string.Empty, RegistrationTypeCode = RegistrationTypeCodes.Group };

        // Act
        var result = await _validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    public async Task Validate_WithEmptyRegistrationTypeCode_And_ValidJoinerCode_IsValid()
    {
        // Arrange
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = string.Empty, RegistrationTypeCode = RegistrationTypeCodes.Individual, LeaverCode = "01" };

        // Act
        var result = await _validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public async Task Validate_WithEmptyRegistrationTypeCode_And_ValidLeaverCode_IsValid()
    {
        // Arrange
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = string.Empty, RegistrationTypeCode = RegistrationTypeCodes.Individual, LeaverCode = "04" };

        // Act
        var result = await _validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public async Task Validate_WithEmptyRegistrationTypeCode_And_ValidJoinerCode_Error()
    {
        // Arrange
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = string.Empty, RegistrationTypeCode = string.Empty, LeaverCode = "01" };

        // Act
        var result = await _validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.RegistrationTypeCode)
            .WithErrorCode(ErrorCodes.RegistrationTypeCodeIsMandatory);
    }

    [TestMethod]
    public async Task Validate_WithEmptyRegistrationTypeCode_And_ValidLeaverCode_Error()
    {
        // Arrange
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = string.Empty, RegistrationTypeCode = string.Empty, LeaverCode = "04" };

        // Act
        var result = await _validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.RegistrationTypeCode)
            .WithErrorCode(ErrorCodes.RegistrationTypeCodeIsMandatory);
    }

    [TestMethod]
    public async Task Validate_WithEmptyRegistrationTypeCode_And_EmptySubsidiaryId_WhenUploadedByCS_IsInvalid()
    {
        // Arrange
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", RegistrationTypeCode = string.Empty, LeaverCode = "01" };

        var validator = new RegistrationTypeCodeValidator(true);

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.RegistrationTypeCode)
            .WithErrorCode(ErrorCodes.RegistrationTypeCodeIsMandatory);
    }

    [TestMethod]
    public async Task Validate_WithValidRegistrationTypeCode_IsValid()
    {
        // Arrange
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", RegistrationTypeCode = RegistrationTypeCodes.Group };

        // Act
        var result = await _validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(RegistrationTypeCodes.Group)]
    [DataRow(RegistrationTypeCodes.Individual)]
    [DataRow("XX")]
    public async Task Validate_RegistrationTypeCode_ShouldValidateCorrectly(string registrationTypeCode)
    {
        // Arrange
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", RegistrationTypeCode = registrationTypeCode };

        // Act
        var result = await _validator.TestValidateAsync(orgDataRow);

        // Assert
        if (registrationTypeCode == RegistrationTypeCodes.Group || registrationTypeCode == RegistrationTypeCodes.Individual)
        {
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.RegistrationTypeCode)
                .WithErrorCode(ErrorCodes.RegistrationTypeCodeInvalidValue);
        }
    }
}
