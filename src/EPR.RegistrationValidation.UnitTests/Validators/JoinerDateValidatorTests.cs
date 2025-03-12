namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class JoinerDateValidatorTests
{
    [TestMethod]
    [DataRow("2000/01/01")]
    [DataRow("2000/20/01")]
    [DataRow("2000/1/1")]
    [DataRow("01-01-2000")]
    [DataRow("01.01.2000")]
    public async Task Validate_WithIncorrectJoinerDateFormat_IsNotValid(string date)
    {
        // Arrange
        var validator = new JoinerDateValidator();
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", JoinerDate = date };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.JoinerDate);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidJoinerDateFormat);
    }

    [TestMethod]
    public async Task Validate_WithFutureJoinerDate_IsInvalid()
    {
        // Arrange
        var validator = new JoinerDateValidator();
        var orgDataRow = new OrganisationDataRow
        {
            JoinerDate = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy"),
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.JoinerDate);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.JoinerDateCannotBeInTheFuture);
    }

    [TestMethod]
    public async Task Validate_WithSubsidiaryIdAndCorrectJoinerDateFormat_IsValid()
    {
        // Arrange
        var validator = new JoinerDateValidator();
        var orgDataRow = new OrganisationDataRow { SubsidiaryId = "1", JoinerDate = "01/01/2000" };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
