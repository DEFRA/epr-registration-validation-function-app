namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class LeaverCodeValidatorTests
{
    [TestMethod]
    public async Task Validate_WithSubsidiaryIdPresentAndLeaverDatePresentEmptyLeaverCode_IsNotValid()
    {
        // Arrange
        var validator = new LeaverCodeValidator();
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", LeaverDate = "22/01/2025" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.LeaverCode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.LeaverCodeMustBePresentWhenLeaverDatePresent);
    }
}
