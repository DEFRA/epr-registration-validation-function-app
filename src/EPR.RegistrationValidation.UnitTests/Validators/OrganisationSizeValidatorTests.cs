namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class OrganisationSizeValidatorTests
{
    [TestMethod]
    public async Task Validate_WithOrganisationSize_IsNull_ReturnError()
    {
        var validator = CreateOrganisationSizeValidator();
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", OrganisationSize = null };

        var result = await validator.TestValidateAsync(orgDataRow);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(err => err.ErrorCode == "894");
    }

    [TestMethod]
    public async Task Validate_WithOrganisationSize_IsInValid_ReturnError()
    {
        var validator = CreateOrganisationSizeValidator();
        var invalidSizeValue = "x";
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", OrganisationSize = invalidSizeValue };

        var result = await validator.TestValidateAsync(orgDataRow);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(err => err.ErrorCode == "895");
    }

    [TestMethod]
    public async Task ValidateDP_WhenOrganisationSizeIsSmall_ReturnCorrectResultBasedOnRegJourney()
    {
        var largeValidator = CreateOrganisationSizeValidator("DirectLargeProducer");
        var smallValidator = CreateOrganisationSizeValidator("DirectSmallProducer");
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", OrganisationSize = "s" };

        var largeResult = await largeValidator.TestValidateAsync(orgDataRow);
        var smallResult = await smallValidator.TestValidateAsync(orgDataRow);

        smallResult.IsValid.Should().BeTrue();
        smallResult.Errors.Should().BeEmpty();

        largeResult.IsValid.Should().BeFalse();
        largeResult.Errors.Should().NotBeEmpty();
        largeResult.Errors.Should().Contain(err => err.ErrorCode == "932");
    }

    [TestMethod]
    public async Task ValidateCS_WhenOrganisationSizeIsSmall_ReturnCorrectResultBasedOnRegJourney()
    {
        var largeValidator = CreateOrganisationSizeValidator("CsoLargeProducer");
        var smallValidator = CreateOrganisationSizeValidator("CsoSmallProducer");
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", OrganisationSize = "s" };

        var largeResult = await largeValidator.TestValidateAsync(orgDataRow);
        var smallResult = await smallValidator.TestValidateAsync(orgDataRow);

        smallResult.IsValid.Should().BeTrue();
        smallResult.Errors.Should().BeEmpty();

        largeResult.IsValid.Should().BeFalse();
        largeResult.Errors.Should().NotBeEmpty();
        largeResult.Errors.Should().Contain(err => err.ErrorCode == "932");
    }

    [TestMethod]
    public async Task ValidateCS_WhenOrganisationSizeIsLarge_ReturnCorrectResultBasedOnRegJourney()
    {
        var largeValidator = CreateOrganisationSizeValidator("CsoLargeProducer");
        var smallValidator = CreateOrganisationSizeValidator("CsoSmallProducer");
        var orgDataRow = new OrganisationDataRow { DefraId = "1234567890", OrganisationSize = "L" };

        var largeResult = await largeValidator.TestValidateAsync(orgDataRow);
        var smallResult = await smallValidator.TestValidateAsync(orgDataRow);

        largeResult.IsValid.Should().BeTrue();
        largeResult.Errors.Should().BeEmpty();

        smallResult.IsValid.Should().BeFalse();
        smallResult.Errors.Should().NotBeEmpty();
        smallResult.Errors.Should().Contain(err => err.ErrorCode == "933");
    }

    private static OrganisationSizeValidator CreateOrganisationSizeValidator(string? registrationJourney = null)
    {
        return new OrganisationSizeValidator(registrationJourney);
    }
}