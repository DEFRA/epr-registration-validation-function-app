namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

[TestClass]
public class OrganisationTradingNameValidatorTests
{
    [TestMethod]
    public async Task WhenDataRowIsValid_ThenValid()
    {
        // Arrange
        var dataRow = RowDataTestHelper.GenerateOrgs(1).First();

        var validator = new OrganisationTradingNameValidator();

        // Act
        var result = await validator.ValidateAsync(dataRow);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public async Task WhenTradingNameNotProvided_ThenValid()
    {
        // Arrange
        var dataRow = RowDataTestHelper.GenerateOrgs(1).First();
        dataRow.TradingName = string.Empty;

        var validator = new OrganisationTradingNameValidator();

        // Act
        var result = await validator.ValidateAsync(dataRow);

        // Assert
        dataRow.OrganisationName.Should().NotBeNullOrEmpty();
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public async Task WhenNeitherTradingNameNorOrganisationNameProvided_ThenValid()
    {
        // Arrange
        var dataRow = RowDataTestHelper.GenerateOrgs(1).First();
        dataRow.OrganisationName = string.Empty;
        dataRow.TradingName = string.Empty;

        var validator = new OrganisationTradingNameValidator();

        // Act
        var result = await validator.TestValidateAsync(dataRow);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public async Task WhenTradingNameSameAsOrganisationName_ThenError()
    {
        // Arrange
        var dataRow = RowDataTestHelper.GenerateOrgs(1).First();
        dataRow.OrganisationName.Should().NotBeNullOrEmpty();
        dataRow.TradingName = dataRow.OrganisationName;

        var validator = new OrganisationTradingNameValidator();

        // Act
        var result = await validator.TestValidateAsync(dataRow);

        // Assert
        result.IsValid.Should().BeFalse();

        result.ShouldHaveValidationErrorFor(organisation => organisation.TradingName);

        result.Errors.Should().ContainSingle(error => error.ErrorCode == ErrorCodes.TradingNameSameAsOrganisationName);
    }
}