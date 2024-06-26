﻿namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class BrandDataRowCharacterLengthValidatorTests
{
    [TestMethod]
    public async Task Validate_WithLongDefraId_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();

        var brandDataRow = CreateBrandDataRow(defraId: new string('x', CharacterLimits.MaxLength + 1));

        // Act
        var result = await validator.TestValidateAsync(brandDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.DefraId);
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(BrandDataRow.DefraId) &&
            x.ErrorCode == ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public async Task Validate_WithLongSubsidiaryId_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();

        var brandDataRow = CreateBrandDataRow(subsidiaryId: new string('x', CharacterLimits.MaxLength + 1));

        // Act
        var result = await validator.TestValidateAsync(brandDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.SubsidiaryId);
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(BrandDataRow.SubsidiaryId) &&
            x.ErrorCode == ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public async Task Validate_WithLongBrandName_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();

        var brandDataRow = CreateBrandDataRow(brandName: new string('x', CharacterLimits.MaxLength + 1));

        // Act
        var result = await validator.TestValidateAsync(brandDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.BrandName);
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(BrandDataRow.BrandName) &&
            x.ErrorCode == ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public async Task Validate_WithLongBrandTypeCode_IsNotValid()
    {
        // Arrange
        var validator = CreateValidator();

        var brandDataRow = CreateBrandDataRow(brandTypeCode: new string('x', CharacterLimits.MaxLength + 1));

        // Act
        var result = await validator.TestValidateAsync(brandDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.BrandTypeCode);
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(BrandDataRow.BrandTypeCode) &&
            x.ErrorCode == ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public async Task Validate_WithMaxLength_IsValid()
    {
        // Arrange
        var validator = CreateValidator();
        var brandDataRow = CreateBrandDataRow(
            defraId: new string('x', CharacterLimits.MaxLength),
            subsidiaryId: new string('x', CharacterLimits.MaxLength),
            brandName: new string('x', CharacterLimits.MaxLength),
            brandTypeCode: new string('x', CharacterLimits.MaxLength));

        // Act
        var result = await validator.TestValidateAsync(brandDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private static BrandDataRowCharacterLengthValidator CreateValidator()
    {
        return new BrandDataRowCharacterLengthValidator();
    }

    private static BrandDataRow CreateBrandDataRow(
        int lineNumber = 1,
        string defraId = "123456",
        string subsidiaryId = null,
        string brandName = null,
        string brandTypeCode = null)
    {
        return new BrandDataRow
        {
            LineNumber = lineNumber,
            DefraId = defraId,
            SubsidiaryId = subsidiaryId,
            BrandName = brandName,
            BrandTypeCode = brandTypeCode,
        };
    }
}