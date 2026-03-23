namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ClosedLoopRegistrationValidatorTests
{
    [TestMethod]
    public async Task Validate_WhenClosedLoopRegistrationIsNull_IsValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow { ClosedLoopRegistration = null };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WhenClosedLoopRegistrationIsEmpty_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow { ClosedLoopRegistration = string.Empty };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ClosedLoopRegistration);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidClosedLoopRegistrationValue);
    }

    [DataTestMethod]
    [DataRow("Yes")]
    [DataRow("yes")]
    [DataRow("YES")]
    public async Task Validate_WhenClosedLoopRegistrationIsYes_IsValid(string value)
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow { ClosedLoopRegistration = value };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [DataTestMethod]
    [DataRow("No")]
    [DataRow("no")]
    [DataRow("NO")]
    public async Task Validate_WhenClosedLoopRegistrationIsNo_IsValid(string value)
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow { ClosedLoopRegistration = value };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [DataTestMethod]
    [DataRow("maybe")]
    [DataRow("true")]
    [DataRow("1")]
    [DataRow("y")]
    [DataRow("n")]
    public async Task Validate_WhenClosedLoopRegistrationIsInvalidValue_IsNotValid(string value)
    {
        // Arrange
        var validator = CreateValidator();
        var orgDataRow = new OrganisationDataRow { ClosedLoopRegistration = value };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ClosedLoopRegistration);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidClosedLoopRegistrationValue);
    }

    private static ClosedLoopRegistrationValidator CreateValidator() => new();
}
