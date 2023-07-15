namespace EPR.RegistrationValidation.UnitTests.TestHelpers;

using Data.Models;

public class CSVRowTestHelper
{
    public static CsvDataRow GenerateCSVDataRowTestHelper(string organisationTypeCode, string packagingActivitySO)
    {
        var csvDataRow = new CsvDataRow
        {
            DefraId = Guid.NewGuid().ToString(),
            OrganisationTypeCode = organisationTypeCode,
            SubsidiaryId = Guid.NewGuid().ToString(),
            PackagingActivitySO = packagingActivitySO,
        };
        return csvDataRow;
    }
}