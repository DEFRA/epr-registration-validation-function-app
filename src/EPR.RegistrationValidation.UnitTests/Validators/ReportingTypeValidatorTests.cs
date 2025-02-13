namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ReportingTypeValidatorTests
{
    [TestMethod]
    public async Task Validate_WithSubsidiaryIdAndEmptyReportingType_IsNotValid()
    {
        // Arrange
        var validator = new ReportingTypeValidator();
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.ReportingType);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.ReportingTypeIsRequired);
    }

    [TestMethod]
    public async Task Validate_WithSubsidiaryIdAndIncorrectReportingTypeValue_IsNotValid()
    {
        // Arrange
        var validator = new ReportingTypeValidator();
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", ReportingType = "Test" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.ReportingType);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidReportingType);
    }

    [TestMethod]
    [DataRow("self")]
    [DataRow("group")]
    public async Task Validate_WithSubsidiaryIdAndReportingType_IsValid(string reportigType)
    {
        // Arrange
        var validator = new ReportingTypeValidator();
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", ReportingType = reportigType };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
