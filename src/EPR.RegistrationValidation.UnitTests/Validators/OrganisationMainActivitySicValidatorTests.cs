namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

[TestClass]
public class OrganisationMainActivitySicValidatorTests
{
    [TestMethod]
    public async Task WhenDataRowIsValid_ThenValid()
    {
        // Arrange
        var dataRow = RowDataTestHelper.GenerateOrgs(1).First();

        var validator = new OrganisationMainActivitySicValidator();

        // Act
        var result = await validator.ValidateAsync(dataRow);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public async Task WhenMainActivitySicNotProvided_ThenValid()
    {
        // Arrange
        var dataRow = RowDataTestHelper.GenerateOrgs(1).First();
        dataRow.MainActivitySic = string.Empty;

        var validator = new OrganisationMainActivitySicValidator();

        // Act
        var result = await validator.ValidateAsync(dataRow);

        // Assert
        dataRow.OrganisationName.Should().NotBeNullOrEmpty();
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public async Task WhenProvidedMainActivitySicNotFiveDigitNumber_ThenError()
    {
        // Arrange
        var dataRow = RowDataTestHelper.GenerateOrgs(1).First();
        dataRow.MainActivitySic = "123456";

        var validator = new OrganisationMainActivitySicValidator();

        // Act
        var result = await validator.TestValidateAsync(dataRow);

        // Assert
        result.IsValid.Should().BeFalse();

        result.ShouldHaveValidationErrorFor(organisation => organisation.MainActivitySic);

        result.Errors.Should().ContainSingle(error => error.ErrorCode == ErrorCodes.MainActivitySicNotFiveDigitsInteger);
    }

    [TestMethod]
    [DataRow("12345")]
    [DataRow("01110")]
    [DataRow("99999")]
    public void WhenStringRepresentsFiveDigitNumber_ThenBeFiveDigitsNumberReturnsTrue(string fiveDigitInteger)
    {
        OrganisationMainActivitySicValidator.BeFiveDigitsNumber(fiveDigitInteger).Should().BeTrue();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("0000A")]
    [DataRow("1234")]
    [DataRow("123456")]
    public void WhenStringDoesNotRepresentFiveDigitNumber_ThenBeFiveDigitsNumberReturnsFalse(string nonFiveDigitText)
    {
        OrganisationMainActivitySicValidator.BeFiveDigitsNumber(nonFiveDigitText).Should().BeFalse();
    }
}