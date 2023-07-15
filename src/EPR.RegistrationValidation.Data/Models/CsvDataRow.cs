namespace EPR.RegistrationValidation.Data.Models;

using CsvHelper.Configuration.Attributes;

public class CsvDataRow
{
    [Index(0)]
    public string DefraId { get; init; }

    [Index(1)]
    public string SubsidiaryId { get; init; }

    [Index(7)]
    public string OrganisationTypeCode { get; init; }

    [Index(9)]
    public string PackagingActivitySO { get; init; }
}