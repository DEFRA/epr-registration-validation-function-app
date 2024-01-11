namespace EPR.RegistrationValidation.Data.Models;

using System.Diagnostics.CodeAnalysis;
using Attributes;
using CsvHelper.Configuration.Attributes;

[ExcludeFromCodeCoverage]
public class PartnersDataRow : ICsvDataRow
{
    [LineNumber]
    public int LineNumber { get; set; }

    [Name("organisation_id")]
    [Column(0)]
    public string DefraId { get; set; }

    [Name("subsidiary_id")]
    [Column(1)]
    public string SubsidiaryId { get; set; }

    [Name("partner_first_name")]
    [Column(2)]
    public string PartnerFirstName { get; set; }

    [Name("partner_last_name")]
    [Column(3)]
    public string PartnerLastName { get; set; }

    [Name("partner_phone_number")]
    [Column(4)]
    public string PartnerPhoneNumber { get; set; }

    [Name("partner_email")]
    [Column(5)]
    public string PartnerEmail { get; set; }
}