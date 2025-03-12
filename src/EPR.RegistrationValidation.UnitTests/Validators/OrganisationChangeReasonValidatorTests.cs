namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class OrganisationChangeReasonValidatorTests
{
    [TestMethod]
    public async Task Validate_WithSubsidiaryIdAndLeaverCodeNotEmptyAndOrganisationChangeReasonGreaterThan200InLength_IsNotValid()
    {
        // Arrange
        var validator = new OrganisationChangeReasonValidator();
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "Any", OrganisationChangeReason = new string('X', 201) };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.OrganisationChangeReason);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.OrganisationChangeReasonCannotBeLongerThan200Characters);
    }

    [TestMethod]
    public async Task Validate_WithSubsidiaryIdAndLeaverCodeNotEmptyAndOrganisationChangeReasonNotGreaterThan200InLength_IsValid()
    {
        // Arrange
        var validator = new OrganisationChangeReasonValidator();
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "Any", OrganisationChangeReason = new string('X', 200) };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithOrganisationChangeReasonContainingSpecialCharacters_IsNotValid()
    {
        // Arrange
        var validator = new OrganisationChangeReasonValidator();
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "Any", OrganisationChangeReason = "Invalid@Reason!" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.OrganisationChangeReason);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.OrganisationChangeReasonCannotIncludeSpecialCharacters);
    }

    [TestMethod]
    public async Task Validate_WithOrganisationChangeReasonContainingOnlyValidCharacters_IsValid()
    {
        // Arrange
        var validator = new OrganisationChangeReasonValidator();
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "Any", OrganisationChangeReason = "Valid Reason123" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
