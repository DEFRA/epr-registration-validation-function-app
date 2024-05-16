namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ServiceOfNoticeAddressValidatorTests
{
    [TestMethod]
    public async Task ServiceOfNoticeAddress_WhenAllFieldsAreEmpty_IsValid()
    {
        // Arrange
        var validator = new ServiceOfNoticeAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            ServiceOfNoticeAddressLine1 = null,
            ServiceOfNoticeAddressLine2 = null,
            ServiceOfNoticeAddressPostcode = null,
            ServiceOfNoticeAddressPhoneNumber = null,
            ServiceOfNoticeAddressCity = null,
            ServiceOfNoticeAddressCounty = null,
            ServiceOfNoticeAddressCountry = null,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ServiceOfNoticeAddress_WithEmptyAddressLine1_IsNotValid()
    {
        // Arrange
        var validator = new ServiceOfNoticeAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "01582",
            ServiceOfNoticeAddressLine1 = null,
            ServiceOfNoticeAddressPostcode = null,
            ServiceOfNoticeAddressPhoneNumber = "0123456789",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.ServiceOfNoticeAddressLine1);
        result.ShouldHaveValidationErrorFor(x => x.ServiceOfNoticeAddressPostcode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingServiceOfNoticeAddressLine1);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingServiceOfNoticePostcode);
    }

    [TestMethod]
    public async Task ServiceOfNoticeAddress_WithEmptyPostCode_IsNotValid()
    {
        // Arrange
        var validator = new ServiceOfNoticeAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "01582",
            ServiceOfNoticeAddressLine1 = "10 Southcote Ave",
            ServiceOfNoticeAddressPostcode = null,
            ServiceOfNoticeAddressPhoneNumber = "0123456789",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.ServiceOfNoticeAddressPostcode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingServiceOfNoticePostcode);
    }

    [TestMethod]
    public async Task ServiceOfNoticeAddress_WithEmptyPhoneNumber_IsNotValid()
    {
        // Arrange
        var validator = new ServiceOfNoticeAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "01582",
            ServiceOfNoticeAddressLine1 = "10 Southcote Ave",
            ServiceOfNoticeAddressPostcode = "KT5 9JT",
            ServiceOfNoticeAddressPhoneNumber = null,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.ServiceOfNoticeAddressPhoneNumber);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingServiceOfNoticePhoneNumber);
    }

    [TestMethod]
    public async Task ServiceOfNoticeAddress_WithoutAnyRequiredFields_IsNotValid()
    {
        // Arrange
        var validator = new ServiceOfNoticeAddressValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "01582",
            ServiceOfNoticeAddressLine1 = null,
            ServiceOfNoticeAddressPostcode = null,
            ServiceOfNoticeAddressPhoneNumber = null,
            ServiceOfNoticeAddressLine2 = "10 Southcote Ave",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.ServiceOfNoticeAddressLine1);
        result.ShouldHaveValidationErrorFor(x => x.ServiceOfNoticeAddressPostcode);
        result.ShouldHaveValidationErrorFor(x => x.ServiceOfNoticeAddressPhoneNumber);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingServiceOfNoticeAddressLine1);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingServiceOfNoticePostcode);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingServiceOfNoticePhoneNumber);
    }
}