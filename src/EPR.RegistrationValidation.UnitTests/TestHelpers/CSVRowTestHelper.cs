namespace EPR.RegistrationValidation.UnitTests.TestHelpers;

using Data.Models;
using EPR.RegistrationValidation.Application.Constants;

public static class CSVRowTestHelper
{
    public static OrganisationDataRow GenerateOrgCsvDataRow(
        string? organisationTypeCode = null,
        string? packagingActivitySo = null)
    {
        var csvDataRow = new OrganisationDataRow
        {
            DefraId = Guid.NewGuid().ToString(),
            OrganisationTypeCode = organisationTypeCode ?? IncorporationTypeCodes.LimitedCompany,
            SubsidiaryId = Guid.NewGuid().ToString(),
            PackagingActivitySO = packagingActivitySo ?? PackagingActivities.No,
        };
        return csvDataRow;
    }

    public static BrandDataRow GenerateBrandCsvDataRow()
    {
        var csvDataRow = new BrandDataRow()
        {
            DefraId = Guid.NewGuid().ToString(),
            SubsidiaryId = Guid.NewGuid().ToString(),
            BrandName = "brandName",
            BrandTypeCode = "brandTypeCode",
        };
        return csvDataRow;
    }

    public static PartnersDataRow GeneratePartnershipCsvDataRow()
    {
        var csvDataRow = new PartnersDataRow
        {
            DefraId = Guid.NewGuid().ToString(),
            SubsidiaryId = Guid.NewGuid().ToString(),
            PartnerFirstName = "partnerFirstName",
            PartnerLastName = "partnerLastName",
            PartnerPhoneNumber = "partnerPhoneNumber",
            PartnerEmail = "partnerEmail",
        };
        return csvDataRow;
    }

    public static OrganisationDataRow GenerateOrgCsvDataRowWithTooManyCharacters(
        string? organisationTypeCode = null,
        string? packagingActivitySo = null)
    {
        var csvDataRow = new OrganisationDataRow
        {
            DefraId = Guid.NewGuid().ToString(),
            OrganisationTypeCode = organisationTypeCode ?? IncorporationTypeCodes.LimitedCompany,
            SubsidiaryId = Guid.NewGuid().ToString(),
            PackagingActivitySO = packagingActivitySo ?? PackagingActivities.No,
            TradingName = new string('a', CharacterLimits.MaxLength + 1),
        };
        return csvDataRow;
    }
}