namespace EPR.RegistrationValidation.Data.Models;

using System.Diagnostics.CodeAnalysis;
using Attributes;
using CsvHelper.Configuration.Attributes;

[ExcludeFromCodeCoverage]
public class BrandDataRow
{
    [Name("organisation_id")]
    [Column(0)]
    public string DefraId { get; set; }

    [Name("subsidiary_id")]
    [Column(1)]
    public string SubsidiaryId { get; set; }

    [Name("brand_name")]
    [Column(2)]
    public string BrandName { get; set; }

    [Name("brand_type_code")]
    [Column(3)]
    public string BrandTypeCode { get; set; }
}