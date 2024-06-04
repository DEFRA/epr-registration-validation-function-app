namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PartnerDataRowCharacterLengthValidatorTests
{
    [TestMethod]
    public async Task Validate_WithLongDefraId_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();

        var partnerDataRow = CreatePartnerDataRow(defraId: new string('x', CharacterLimits.MaxLength + 1));

        // Act
        var result = await validator.TestValidateAsync(partnerDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.DefraId);
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(PartnersDataRow.DefraId) &&
            x.ErrorCode == ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public async Task Validate_WithLongSubsidiaryId_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();

        var partnerDataRow = CreatePartnerDataRow(subsidiaryId: new string('x', CharacterLimits.MaxLength + 1));

        // Act
        var result = await validator.TestValidateAsync(partnerDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.SubsidiaryId);
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(PartnersDataRow.SubsidiaryId) &&
            x.ErrorCode == ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public async Task Validate_WithLongPartnerFirstName_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();

        var partnerDataRow = CreatePartnerDataRow(partnerFirstName: new string('x', CharacterLimits.MaxLength + 1));

        // Act
        var result = await validator.TestValidateAsync(partnerDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PartnerFirstName);
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(PartnersDataRow.PartnerFirstName) &&
            x.ErrorCode == ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public async Task Validate_WithLongPartnerLastName_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();

        var partnerDataRow = CreatePartnerDataRow(partnerLastName: new string('x', CharacterLimits.MaxLength + 1));

        // Act
        var result = await validator.TestValidateAsync(partnerDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PartnerLastName);
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(PartnersDataRow.PartnerLastName) &&
            x.ErrorCode == ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public async Task Validate_WithLongPartnerPhoneNumber_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();

        var partnerDataRow = CreatePartnerDataRow(partnerPhoneNumber: new string('x', CharacterLimits.MaxLength + 1));

        // Act
        var result = await validator.TestValidateAsync(partnerDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PartnerPhoneNumber);
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(PartnersDataRow.PartnerPhoneNumber) &&
            x.ErrorCode == ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public async Task Validate_WithLongPartnerEmail_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();

        var partnerDataRow = CreatePartnerDataRow(partnerEmail: new string('x', CharacterLimits.MaxLength + 1));

        // Act
        var result = await validator.TestValidateAsync(partnerDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.PartnerEmail);
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(PartnersDataRow.PartnerEmail) &&
            x.ErrorCode == ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public async Task Validate_WithMaxLength_IsValid()
    {
        // Arrange
        var validator = CreateValidator();
        var partnerDataRow = CreatePartnerDataRow(
            defraId: new string('x', CharacterLimits.MaxLength),
            subsidiaryId: new string('x', CharacterLimits.MaxLength),
            partnerFirstName: new string('x', CharacterLimits.MaxLength),
            partnerLastName: new string('x', CharacterLimits.MaxLength),
            partnerPhoneNumber: new string('x', CharacterLimits.MaxLength),
            partnerEmail: new string('x', CharacterLimits.MaxLength));

        // Act
        var result = await validator.TestValidateAsync(partnerDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private static PartnerDataRowCharacterLengthValidator CreateValidator()
    {
        return new PartnerDataRowCharacterLengthValidator();
    }

    private static PartnersDataRow CreatePartnerDataRow(
        int lineNumber = 1,
        string defraId = "123456",
        string subsidiaryId = null,
        string partnerFirstName = null,
        string partnerLastName = null,
        string partnerPhoneNumber = null,
        string partnerEmail = null)
    {
        return new PartnersDataRow
        {
            LineNumber = lineNumber,
            DefraId = defraId,
            SubsidiaryId = subsidiaryId,
            PartnerFirstName = partnerFirstName,
            PartnerLastName = partnerLastName,
            PartnerPhoneNumber = partnerPhoneNumber,
            PartnerEmail = partnerEmail,
        };
    }
}