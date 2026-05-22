namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class SmallProducerClosedLoopRegistrationValidatorTests
{
    [DataTestMethod]
    [DataRow("S", "Yes")]
    [DataRow("S", "yes")]
    [DataRow("S", "YES")]
    [DataRow("s", "Yes")]
    public async Task Validate_WhenSmallProducerAndClosedLoopYes_IsNotValid(string size, string closedLoop)
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow { OrganisationSize = size, ClosedLoopRegistration = closedLoop };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.ClosedLoopRegistrationNotAllowedForSmallProducer);
    }

    [DataTestMethod]
    [DataRow("S", "No")]
    [DataRow("S", "no")]
    [DataRow("L", "Yes")]
    [DataRow("L", "No")]
    public async Task Validate_WhenNotSmallProducerOrClosedLoopNotYes_IsValid(string size, string closedLoop)
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow { OrganisationSize = size, ClosedLoopRegistration = closedLoop };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [DataTestMethod]
    [DataRow("S", null)]
    [DataRow("S", "")]
    [DataRow("S", " ")]
    [DataRow(null, "Yes")]
    [DataRow("", "Yes")]
    [DataRow(" ", "Yes")]
    [DataRow(null, null)]
    public async Task Validate_WhenEitherFieldMissing_IsValid(string size, string closedLoop)
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow { OrganisationSize = size, ClosedLoopRegistration = closedLoop };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private static SmallProducerClosedLoopRegistrationValidator CreateValidator() => new();
}
