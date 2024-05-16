namespace EPR.RegistrationValidation.UnitTests.Validators;

using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.Data.Models.OrganisationDataLookup;
using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PartnerDataRowOrganisationDataValidatorTests
{
    [TestMethod]
    public async Task Validate_WithNonMatchingDefraId_IsNotValid()
    {
        // Arrange
        const string defraId = "123456";
        const string invalidDefraId = "654321";
        var validator = CreateValidator();
        var partnerDataRow = CreatePartnerDataRow(invalidDefraId);

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

        var context = new ValidationContext<PartnersDataRow>(partnerDataRow);
        context.RootContextData[nameof(OrganisationDataLookupTable)] = lookupTable;

        // Act
        var result = await validator.TestValidateAsync(context);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldHaveValidationErrorFor(x => x.DefraId);
    }

    [TestMethod]
    public async Task Validate_WithMatchingDefraId_And_Subsidiary_IsValid()
    {
        // Arrange
        const string defraId = "123456";
        const string subsidiaryId = "000001";
        var validator = CreateValidator();
        var partnerDataRow = CreatePartnerDataRow(defraId, subsidiaryId);

        var lookupTable = new OrganisationDataLookupTable(
            new Dictionary<string, Dictionary<string?, OrganisationIdentifiers>>
            {
                {
                    defraId, new()
                    {
                        { subsidiaryId, new OrganisationIdentifiers(defraId, subsidiaryId) },
                    }
                },
            });

        var context = new ValidationContext<PartnersDataRow>(partnerDataRow);
        context.RootContextData[nameof(OrganisationDataLookupTable)] = lookupTable;

        // Act
        var result = await validator.TestValidateAsync(context);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithMatchingDefraId_And_Empty_Subsidiary_IsValid()
    {
        // Arrange
        const string defraId = "123456";
        const string subsidiaryId = "";
        var validator = CreateValidator();
        var partnerDataRow = CreatePartnerDataRow(defraId, subsidiaryId);

        var lookupTable = new OrganisationDataLookupTable(
            new Dictionary<string, Dictionary<string?, OrganisationIdentifiers>>
            {
                {
                    defraId, new()
                    {
                        { subsidiaryId, new OrganisationIdentifiers(defraId, subsidiaryId) },
                    }
                },
            });

        var context = new ValidationContext<PartnersDataRow>(partnerDataRow);
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
        var partnerDataRow = CreatePartnerDataRow(defraId);

        // Act
        var result = await validator.TestValidateAsync(partnerDataRow);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    private static PartnerDataRowOrganisationDataValidator CreateValidator()
    {
        return new PartnerDataRowOrganisationDataValidator();
    }

    private static PartnersDataRow CreatePartnerDataRow(
        string defraId,
        string subsidiaryId = null)
    {
        return new PartnersDataRow()
        {
            DefraId = defraId,
            SubsidiaryId = subsidiaryId,
            PartnerFirstName = "PartnerFirstName",
            PartnerLastName = "PartnerLastName",
            PartnerPhoneNumber = "PartnerPhoneNumber",
            PartnerEmail = "PartnerEmail",
        };
    }
}