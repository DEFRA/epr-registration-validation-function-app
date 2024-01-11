namespace EPR.RegistrationValidation.UnitTests.TestHelpers;

using Data.Models;

public class CSVRowTestHelper
{
    public static OrganisationDataRow GenerateCSVDataRowTestHelper(string organisationTypeCode, string packagingActivitySO)
    {
        var csvDataRow = new OrganisationDataRow
        {
            DefraId = Guid.NewGuid().ToString(),
            OrganisationTypeCode = organisationTypeCode,
            SubsidiaryId = Guid.NewGuid().ToString(),
            PackagingActivitySO = packagingActivitySO,
        };
        return csvDataRow;
    }

    public static BrandDataRow GenerateBrandCSVDataRowTestHelper()
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

    public static PartnersDataRow GeneratePartnershipCSVDataRowTestHelper()
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
}