namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class TotalTonnageValidatorTest
{
    [TestMethod]
    public async Task WhenTotalTonnageIsNegativeNumber_ThenError()
    {
        // Arrange
        var validator = new TotalTonnageValidator();
        var orgDataRow = new OrganisationDataRow
        {
            TotalTonnage = "-100",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TotalTonnage);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.TotalTonnageMustBeGreaterThanZero);
    }

    [TestMethod]
    public async Task WhenTotalTonnageIsZero_ThenError()
    {
        // Arrange
        var validator = new TotalTonnageValidator();
        var orgDataRow = new OrganisationDataRow
        {
            TotalTonnage = "0",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TotalTonnage);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.TotalTonnageMustBeGreaterThanZero);
    }

    [TestMethod]
    public async Task WhenTotalTonnageHasMultiplyZero_ThenError()
    {
        // Arrange
        var validator = new TotalTonnageValidator();
        var orgDataRow = new OrganisationDataRow
        {
            TotalTonnage = "00000",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TotalTonnage);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.TotalTonnageMustBeGreaterThanZero);
    }

    [TestMethod]
    public async Task WhenTotalTonnageContainsComma_ThenError()
    {
        // Arrange
        var validator = new TotalTonnageValidator();
        var orgDataRow = new OrganisationDataRow
        {
            TotalTonnage = "25,5",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TotalTonnage);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.TotalTonnageIncludesComma);
    }

    [TestMethod]
    [DataRow("25.5")]
    [DataRow("15")]
    [DataRow("0.6")]
    public async Task WhenTotalTonnageIsDouble_IsValid(string input)
    {
        // Arrange
        var validator = new TotalTonnageValidator();
        var orgDataRow = new OrganisationDataRow
        {
            TotalTonnage = input,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("0 5")]
    [DataRow("1 1")]
    [DataRow("£1.10")]
    [DataRow("ABC123")]
    [DataRow(" 14 ")]
    public async Task WhenTotalTonnageContainsNonNumeric_ThenError(string input)
    {
        // Arrange
        var validator = new TotalTonnageValidator();
        var orgDataRow = new OrganisationDataRow
        {
            TotalTonnage = input,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TotalTonnage);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.TotalTonnageIsNotNumber);
    }

    [TestMethod]
    public async Task WhenTotalTonnageNotProvided_ThenError()
    {
        // Arrange
        var validator = new TotalTonnageValidator();
        var orgDataRow = new OrganisationDataRow
        {
            TotalTonnage = string.Empty,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TotalTonnage);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.TotalTonnageMustBeProvided);
    }
}