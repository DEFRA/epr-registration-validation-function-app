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
}