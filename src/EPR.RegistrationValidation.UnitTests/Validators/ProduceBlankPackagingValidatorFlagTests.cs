namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ProduceBlankPackagingValidatorFlagTests
{
    [TestMethod]
    public async Task Validate_WithProduceBlankPackagingFlagIsNullWhenPackagingActivitySoIsPrimary_IsNotValid()
    {
        // Arrange
        var validator = new ProduceBlankPackagingValidator();
        var orgDataRow = new OrganisationDataRow
        {
            PackagingActivitySO = PackagingActivities.Primary,
            ProduceBlankPackagingFlag = null,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ProduceBlankPackagingFlag);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidProduceBlankPackagingFlag);
    }

    [TestMethod]
    public async Task Validate_WithInvalidProduceBlankPackagingFlagWhenBrandOwner_IsNotValid()
        {
            // Arrange
            var validator = new ProduceBlankPackagingValidator();
            var orgDataRow = new OrganisationDataRow
            {
                DefraId = "01582",
                PackagingActivitySO = PackagingActivities.Primary,
                ProduceBlankPackagingFlag = "test",
            };

            // Act
            var result = await validator.TestValidateAsync(orgDataRow);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.ShouldHaveValidationErrorFor(x => x.ProduceBlankPackagingFlag);
            result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidProduceBlankPackagingFlag);
        }

    [TestMethod]
    public async Task Validate_WithInvalidProduceBlankPackagingFlagWhenNotBrandOwner_IsNotValid()
    {
        // Arrange
        var validator = new ProduceBlankPackagingValidator();
        var orgDataRow = new OrganisationDataRow
        {
            DefraId = "01582",
            PackagingActivitySO = PackagingActivities.No,
            ProduceBlankPackagingFlag = "test",
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.ProduceBlankPackagingFlag);
        result.Errors.Should().Contain(err => err.ErrorCode == ErrorCodes.InvalidProduceBlankPackagingFlag);
    }

    [TestMethod]
    public async Task Validate_ProduceBlankPackagingFlag_IsValid()
    {
        // Arrange
        var validator = new ProduceBlankPackagingValidator();
        var orgDataRow = new OrganisationDataRow
        {
            PackagingActivitySO = PackagingActivities.Primary,
            ProduceBlankPackagingFlag = YesNoOption.Yes,
        };

        // Act
        var result = await validator.TestValidateAsync(orgDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Errors.Should().NotContain(err => err.ErrorCode == ErrorCodes.InvalidProduceBlankPackagingFlag);
        }
}
