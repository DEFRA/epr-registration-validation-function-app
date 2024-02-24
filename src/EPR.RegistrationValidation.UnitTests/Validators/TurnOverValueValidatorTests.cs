namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class TurnOverValueValidatorTests
{
    [TestMethod]
    public async Task WhenTurnoverNotGreaterThanZero_ThenError()
    {
        // Arrange
        var validator = new TurnoverValueValidator();
        var orgDataRow = new OrganisationDataRow
        {
            Turnover = "-101",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Turnover);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.TurnoverHasZeroOrNegativeValue);
    }

    [TestMethod]
    public async Task WhenTurnoverIncludesComma_ThenError()
    {
        // Arrange
        var validator = new TurnoverValueValidator();
        var orgDataRow = new OrganisationDataRow
        {
            Turnover = "10,000",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Turnover);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.TurnoverHasComma);
    }

    [TestMethod]
    public async Task WhenTurnoverIsZero_ThenError()
    {
        // Arrange
        var validator = new TurnoverValueValidator();
        var orgDataRow = new OrganisationDataRow
        {
            Turnover = "000",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Turnover);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.TurnoverHasZeroOrNegativeValue);
    }

    [TestMethod]
    [DataRow("12")]
    [DataRow("12.5")]
    [DataRow("12.55")]
    [DataRow("0.55")]
    [DataRow("99999999999.99")]
    public async Task Validate_Turnover_IsValid(string turnoverValue)
    {
        // Arrange
        var validator = new TurnoverValueValidator();
        var orgDataRow = new OrganisationDataRow
        {
            Turnover = turnoverValue,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("1 2")]
    [DataRow("12u55")]
    [DataRow(" 12345 ")]
    [DataRow("£12345")]
    public async Task WhenTurnoverHasInvalidDigits_ThenError(string turnoverValue)
    {
        // Arrange
        var validator = new TurnoverValueValidator();
        var orgDataRow = new OrganisationDataRow
        {
            Turnover = turnoverValue,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Turnover);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.InvalidTurnoverDigits);
    }

    [TestMethod]
    [DataRow("12.999")]
    public async Task Validate_InvalidTurnoverDecimalValues_IsNotValid(string turnoverValue)
    {
        // Arrange
        var validator = new TurnoverValueValidator();
        var orgDataRow = new OrganisationDataRow
        {
            Turnover = turnoverValue,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Turnover);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.InvalidTurnoverDecimalValues);
    }
}