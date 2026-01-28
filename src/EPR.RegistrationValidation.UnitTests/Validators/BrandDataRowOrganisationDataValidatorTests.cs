namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.Data.Models.OrganisationDataLookup;
using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class BrandDataRowOrganisationDataValidatorTests
{
    [TestMethod]
    public async Task Validate_WithNonMatchingDefraId_IsNotValid()
    {
        // Arrange
        const string defraId = "123456";
        const string invalidDefraId = "654321";
        var validator = CreateValidator();
        var brandDataRow = CreateBrandDataRow(invalidDefraId);

        var lookupTable = new OrganisationDataLookupTable(
            new Dictionary<string, Dictionary<string, OrganisationIdentifiers>>
            {
                {
                    defraId, new()
                    {
                        { string.Empty, new OrganisationIdentifiers(defraId, null) },
                    }
                },
            });

        var context = new ValidationContext<BrandDataRow>(brandDataRow);
        context.RootContextData[nameof(OrganisationDataLookupTable)] = lookupTable;

        // Act
        var result = await validator.TestValidateAsync(context);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.DefraId);
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(BrandDataRow.DefraId) &&
            x.ErrorCode == ErrorCodes.BrandDetailsNotMatchingOrganisation);
    }

    [TestMethod]
    public async Task Validate_WithMatchingDefraId_And_Subsidiary_IsValid()
    {
        // Arrange
        const string defraId = "123456";
        const string subsidiaryId = "000001";
        var validator = CreateValidator();
        var brandDataRow = CreateBrandDataRow(defraId, subsidiaryId);

        var lookupTable = new OrganisationDataLookupTable(
            new Dictionary<string, Dictionary<string, OrganisationIdentifiers>>
            {
                {
                    defraId, new()
                    {
                        { subsidiaryId, new OrganisationIdentifiers(defraId, subsidiaryId) },
                    }
                },
            });

        var context = new ValidationContext<BrandDataRow>(brandDataRow);
        context.RootContextData[nameof(OrganisationDataLookupTable)] = lookupTable;

        // Act
        var result = await validator.TestValidateAsync(context);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithMatchingDefraId_And_EmptySubsidiary_IsValid()
    {
        // Arrange
        const string defraId = "123456";
        const string subsidiaryId = "";
        var validator = CreateValidator();
        var brandDataRow = CreateBrandDataRow(defraId, subsidiaryId);

        var lookupTable = new OrganisationDataLookupTable(
            new Dictionary<string, Dictionary<string, OrganisationIdentifiers>>
            {
                {
                    defraId, new()
                    {
                        { subsidiaryId, new OrganisationIdentifiers(defraId, subsidiaryId) },
                    }
                },
            });

        var context = new ValidationContext<BrandDataRow>(brandDataRow);
        context.RootContextData[nameof(OrganisationDataLookupTable)] = lookupTable;

        // Act
        var result = await validator.TestValidateAsync(context);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_With_NoLookupTableInContext_IsValid()
    {
        // Arrange
        const string defraId = "123456";
        var validator = CreateValidator();
        var brandDataRow = CreateBrandDataRow(defraId);

        // Act
        var result = await validator.TestValidateAsync(brandDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private static BrandDataRowOrganisationDataValidator CreateValidator()
    {
        return new BrandDataRowOrganisationDataValidator();
    }

    private static BrandDataRow CreateBrandDataRow(
        string defraId,
        string subsidiaryId = null)
    {
        return new BrandDataRow
        {
            DefraId = defraId,
            SubsidiaryId = subsidiaryId,
            BrandName = "BrandName",
            BrandTypeCode = "BrandTypeCode",
        };
    }
}