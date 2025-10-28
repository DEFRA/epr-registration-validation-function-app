namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class OrganisationSizeTurnoverValidatorTests
{
    [TestMethod]
    [DataRow(OrganisationSizeCodes.S, "1.9", "49.1", true, 0)]
    [DataRow(OrganisationSizeCodes.S, "2.1", "50.1", false, 1)]
    [DataRow(OrganisationSizeCodes.L, "2.1", "50.1", true, 0)]
    [DataRow(OrganisationSizeCodes.L, "1.9", "49.1", true, 0)]
    [DataRow(OrganisationSizeCodes.L, "1.9", "50.1", true, 0)]
    [DataRow(OrganisationSizeCodes.L, "2.1", "49.1", true, 0)]
    public async Task Validate_OrganisationSize_Turnover(OrganisationSizeCodes organisationSize, string turnover, string totalTonnage, bool testResult, int errorCount)
    {
        // Arrange
        var validator = CreateOrganisationSizeTurnoverValidator();
        var orgDataRow = new OrganisationDataRow { OrganisationSize = organisationSize.ToString(), Turnover = turnover, TotalTonnage = totalTonnage };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().Be(testResult);

        // result.Errors.Should().NotBeEmpty();
        result.Errors.Count().Should().Be(errorCount);
    }

    [TestMethod]
    [DataRow(OrganisationSizeCodes.S, "X", "49.1", false, 1)]
    [DataRow(OrganisationSizeCodes.S, "1.9", "YY", false, 1)]
    [DataRow(OrganisationSizeCodes.S, "X", "Y", false, 1)]
    [DataRow(OrganisationSizeCodes.L, "X", "50.1", false, 1)]
    [DataRow(OrganisationSizeCodes.L, "1.9", "Y", false, 1)]
    public async Task Validate_OrganisationSize_Turnover_InvalidInput(OrganisationSizeCodes organisationSize, string turnover, string totalTonnage, bool testResult, int errorCount)
    {
        // Arrange
        var validator = CreateOrganisationSizeTurnoverValidator();
        var orgDataRow = new OrganisationDataRow { OrganisationSize = organisationSize.ToString(), Turnover = turnover, TotalTonnage = totalTonnage };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().Be(testResult);
        result.Errors.Count.Should().Be(errorCount);
    }

    private static OrganisationSizeTurnoverValidator CreateOrganisationSizeTurnoverValidator()
    {
        return new OrganisationSizeTurnoverValidator();
    }
}