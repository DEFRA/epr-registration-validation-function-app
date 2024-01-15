namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PackagingActivitiesValidatorTests
{
    [TestMethod]
    public async Task Validate_WithNullPackagingActivity_IsNotValid()
    {
        // Arrange
        var validator = new PackagingActivityValidator();
        var orgDataRow = new OrganisationDataRow();

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivitySO);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivityHl);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivityPf);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivitySl);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivityIm);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivitySe);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivityOm);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPackagingActivitySo);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPackagingActivityHl);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPackagingActivityPf);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPackagingActivitySl);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPackagingActivityIm);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPackagingActivitySe);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.MissingPackagingActivityOm);
    }

    [TestMethod]
    public async Task Validate_WithValidPackagingActivities_IsValid()
    {
        // Arrange
        var validator = new PackagingActivityValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "1234567890",
            OrganisationName = "AAA ltd",
            HomeNationCode = "EN",
            PrimaryContactPersonLastName = "LName",
            PrimaryContactPersonFirstName = "Fname",
            PrimaryContactPersonEmail = "test@test.com",
            PrimaryContactPersonPhoneNumber = "01237946",
            PackagingActivitySO = "Primary",
            PackagingActivityHl = "Secondary",
            PackagingActivityPf = "Secondary",
            PackagingActivitySl = "Secondary",
            PackagingActivityIm = "No",
            PackagingActivityOm = "No",
            PackagingActivitySe = "Secondary",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithInvalidPackagingActivities_IsNotValid()
    {
        // Arrange
        var validator = new PackagingActivityValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "1234567890",
            OrganisationName = "AAA ltd",
            HomeNationCode = "EN",
            PrimaryContactPersonLastName = "LName",
            PrimaryContactPersonFirstName = "Fname",
            PrimaryContactPersonEmail = "test@test.com",
            PrimaryContactPersonPhoneNumber = "01237946",
            PackagingActivitySO = "Primary",
            PackagingActivityHl = "Secondary",
            PackagingActivityPf = "Secondary",
            PackagingActivitySl = "test",
            PackagingActivityIm = "No",
            PackagingActivityOm = "No",
            PackagingActivitySe = "Secondary",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivitySl);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.InvalidPackagingActivity);
    }

    [TestMethod]
    public async Task Validate_WithMultiplePrimaryPackagingActivities_IsNotValid()
    {
        // Arrange
        var validator = new PackagingActivityValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "1234567890",
            OrganisationName = "AAA ltd",
            HomeNationCode = "EN",
            PrimaryContactPersonLastName = "LName",
            PrimaryContactPersonFirstName = "Fname",
            PrimaryContactPersonEmail = "test@test.com",
            PrimaryContactPersonPhoneNumber = "01237946",
            PackagingActivitySO = "Primary",
            PackagingActivityHl = "Secondary",
            PackagingActivityPf = "Secondary",
            PackagingActivitySl = "Primary",
            PackagingActivityIm = "No",
            PackagingActivityOm = "No",
            PackagingActivitySe = "Secondary",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivitySl);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivitySO);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.MultiplePrimaryActivity);
    }

    [TestMethod]
    public async Task Validate_WithMissingPrimaryPackagingActivities_IsNotValid()
    {
        // Arrange
        var validator = new PackagingActivityValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "1234567890",
            OrganisationName = "AAA ltd",
            HomeNationCode = "EN",
            PrimaryContactPersonLastName = "LName",
            PrimaryContactPersonFirstName = "Fname",
            PrimaryContactPersonEmail = "test@test.com",
            PrimaryContactPersonPhoneNumber = "01237946",
            PackagingActivitySO = "Secondary",
            PackagingActivityHl = "Secondary",
            PackagingActivityPf = "Secondary",
            PackagingActivitySl = "Secondary",
            PackagingActivityIm = "No",
            PackagingActivityOm = "No",
            PackagingActivitySe = "Secondary",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivitySl);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivitySO);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivityHl);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivityPf);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivityIm);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivitySe);
        result.ShouldHaveValidationErrorFor(x => x.PackagingActivityOm);
        result.Errors.Should().OnlyContain(err => err.ErrorCode == ErrorCodes.MissingPrimaryActivity);
    }
}