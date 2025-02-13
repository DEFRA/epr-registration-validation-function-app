namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class LeaverReasonValidatorTests
{
    [TestMethod]
    public async Task Validate_WithSubsidiaryIdAndLeaverCodeNotEmptyAndLeaverReasonGreaterThan200InLength_IsNotValid()
    {
        // Arrange
        var validator = new LeaverReasonValidator();
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "Any", LeaverReason = new string('X', 201) };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverReason);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.LeaverReasonExceedsTwoHundredCharacterLimit);
    }

    [TestMethod]
    public async Task Validate_WithSubsidiaryIdAndLeaverCodeNotEmptyAndLeaverReasonNotGreaterThan200InLength_IsValid()
    {
        // Arrange
        var validator = new LeaverReasonValidator();
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverCode = "Any", LeaverReason = new string('X', 200) };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
