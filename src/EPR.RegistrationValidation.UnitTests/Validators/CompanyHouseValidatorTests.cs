namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

[TestClass]
public class CompanyHouseValidatorTests
{
    [Test]
    [TestCaseSource(nameof(TestCaseSourceData))]
    public async Task Validate_WithEmptyCompanyHouseNumberWithSomeOrganisationType_IsValid(string code)
    {
        // Arrange
        var validator = new CompanyHouseValidator();
        var orgDataRow = new OrganisationDataRow
        {
            CompaniesHouseNumber = string.Empty,
            OrganisationTypeCode = code,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithValidCompanyHouseNumber_IsValid()
    {
        // Arrange
        var validator = new CompanyHouseValidator();
        var orgDataRow = new OrganisationDataRow
        {
            CompaniesHouseNumber = "76523145",
            OrganisationTypeCode = "LTD",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithInvalidCompanyHouseNumber_IsNotValid()
    {
        // Arrange
        var validator = new CompanyHouseValidator();
        var orgDataRow = new OrganisationDataRow
        {
            CompaniesHouseNumber = "00000000",
            OrganisationTypeCode = "LTD",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.CompaniesHouseNumber);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidCompanyHouseNumber);
    }

    [Test]
    [TestCaseSource(nameof(TestCaseSourceData))]
    public async Task Validate_WithValidCompanyHouseNumberForOrganisationType_IsNotValid(string organisationTypeCode)
    {
        // Arrange
        var validator = new CompanyHouseValidator();
        var orgDataRow = new OrganisationDataRow
        {
            CompaniesHouseNumber = "98765432",
            OrganisationTypeCode = organisationTypeCode,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.CompaniesHouseNumber);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.CompanyHouseNumberMustBeEmpty);
    }

    private static IEnumerable<TestCaseData> TestCaseSourceData()
    {
        yield return new TestCaseData(UnIncorporationTypeCodes.Partnership);
        yield return new TestCaseData(UnIncorporationTypeCodes.CoOperative);
        yield return new TestCaseData(UnIncorporationTypeCodes.Others);
        yield return new TestCaseData(UnIncorporationTypeCodes.OutsideUk);
        yield return new TestCaseData(UnIncorporationTypeCodes.SoleTrader);
    }
}