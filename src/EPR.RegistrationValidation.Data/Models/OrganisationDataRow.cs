namespace EPR.RegistrationValidation.Data.Models;

using System.Diagnostics.CodeAnalysis;
using Attributes;
using CsvHelper.Configuration.Attributes;

[ExcludeFromCodeCoverage]
public class OrganisationDataRow : ICsvDataRow
{
    [LineNumber]
    public int LineNumber { get; set; }

    [Name("organisation_id")]
    [Column(0)]
    public string DefraId { get; set; }

    [Name("subsidiary_id")]
    [Column(1)]
    public string SubsidiaryId { get; set; }

    [Name("organisation_name")]
    [Column(2)]
    public string OrganisationName { get; set; }

    [Name("trading_name")]
    [Column(3)]
    public string TradingName { get; set; }

    [Column(4)]
    [Name("companies_house_number")]
    public string CompaniesHouseNumber { get; set; }

    [Column(5)]
    [Name("home_nation_code")]
    public string HomeNationCode { get; set; }

    [Column(6)]
    [Name("main_activity_sic")]
    public string MainActivitySic { get; set; }

    [Column(7)]
    [Name("organisation_type_code")]
    public string OrganisationTypeCode { get; set; }

    [Column(8)]
    [Name("organisation_sub_type_code")]
    public string OrganisationSubTypeCode { get; set; }

    [Column(9)]
    [Name("packaging_activity_so")]
    public string PackagingActivitySO { get; set; }

    [Column(10)]
    [Name("packaging_activity_pf")]
    public string PackagingActivityPf { get; set; }

    [Column(11)]
    [Name("packaging_activity_im")]
    public string PackagingActivityIm { get; set; }

    [Column(12)]
    [Name("packaging_activity_se")]
    public string PackagingActivitySe { get; set; }

    [Column(13)]
    [Name("packaging_activity_hl")]
    public string PackagingActivityHl { get; set; }

    [Column(14)]
    [Name("packaging_activity_om")]
    public string PackagingActivityOm { get; set; }

    [Column(15)]
    [Name("packaging_activity_sl")]
    public string PackagingActivitySl { get; set; }

    [Column(16)]
    [Name("registration_type_code")]
    public string RegistrationTypeCode { get; set; }

    [Column(17)]
    [Name("turnover")]
    public string Turnover { get; set; }

    [Column(18)]
    [Name("total_tonnage")]
    public string TotalTonnage { get; set; }

    [Column(19)]
    [Name("produce_blank_packaging_flag")]
    public string ProduceBlankPackagingFlag { get; set; }

    [Column(20)]
    [Name("liable_for_disposal_costs_flag")]
    public string LiableForDisposalCostsFlag { get; set; }

    [Column(21)]
    [Name("meet_reporting_requirements_flag")]
    public string MeetReportingRequirementsFlag { get; set; }

    [Column(22)]
    [Name("registered_addr_line1")]
    public string RegisteredAddressLine1 { get; set; }

    [Column(23)]
    [Name("registered_addr_line2")]
    public string RegisteredAddressLine2 { get; set; }

    [Column(24)]
    [Name("registered_city")]
    public string RegisteredCity { get; set; }

    [Column(25)]
    [Name("registered_addr_county")]
    public string RegisteredAddressCounty { get; set; }

    [Column(26)]
    [Name("registered_addr_postcode")]
    public string RegisteredAddressPostcode { get; set; }

    [Column(27)]
    [Name("registered_addr_country")]
    public string RegisteredAddressCountry { get; set; }

    [Column(28)]
    [Name("registered_addr_phone_number")]
    public string RegisteredAddressPhoneNumber { get; set; }

    [Column(29)]
    [Name("audit_addr_line1")]
    public string AuditAddressLine1 { get; set; }

    [Column(30)]
    [Name("audit_addr_line2")]
    public string AuditAddressLine2 { get; set; }

    [Column(31)]
    [Name("audit_addr_city")]
    public string AuditAddressCity { get; set; }

    [Column(32)]
    [Name("audit_addr_county")]
    public string AuditAddressCounty { get; set; }

    [Column(33)]
    [Name("audit_addr_postcode")]
    public string AuditAddressPostcode { get; set; }

    [Column(34)]
    [Name("audit_addr_country")]
    public string AuditAddressCountry { get; set; }

    [Column(35)]
    [Name("service_of_notice_addr_line1")]
    public string ServiceOfNoticeAddressLine1 { get; set; }

    [Column(36)]
    [Name("service_of_notice_addr_line2")]
    public string ServiceOfNoticeAddressLine2 { get; set; }

    [Column(37)]
    [Name("service_of_notice_addr_city")]
    public string ServiceOfNoticeAddressCity { get; set; }

    [Column(38)]
    [Name("service_of_notice_addr_county")]
    public string ServiceOfNoticeAddressCounty { get; set; }

    [Column(39)]
    [Name("service_of_notice_addr_postcode")]
    public string ServiceOfNoticeAddressPostcode { get; set; }

    [Column(40)]
    [Name("service_of_notice_addr_country")]
    public string ServiceOfNoticeAddressCountry { get; set; }

    [Column(41)]
    [Name("service_of_notice_addr_phone_number")]
    public string ServiceOfNoticeAddressPhoneNumber { get; set; }

    [Column(42)]
    [Name("principal_addr_line1")]
    public string PrincipalAddressLine1 { get; set; }

    [Column(43)]
    [Name("principal_addr_line2")]
    public string PrincipalAddressLine2 { get; set; }

    [Column(44)]
    [Name("principal_addr_city")]
    public string PrincipalAddressCity { get; set; }

    [Column(45)]
    [Name("principal_addr_county")]
    public string PrincipalAddressCounty { get; set; }

    [Column(46)]
    [Name("principal_addr_postcode")]
    public string PrincipalAddressPostcode { get; set; }

    [Column(47)]
    [Name("principal_addr_country")]
    public string PrincipalAddressCountry { get; set; }

    [Column(48)]
    [Name("principal_addr_phone_number")]
    public string PrincipalAddressPhoneNumber { get; set; }

    [Column(49)]
    [Name("sole_trader_first_name")]
    public string SoleTraderFirstName { get; set; }

    [Column(50)]
    [Name("sole_trader_last_name")]
    public string SoleTraderLastName { get; set; }

    [Column(51)]
    [Name("sole_trader_phone_number")]
    public string SoleTraderPhoneNumber { get; set; }

    [Column(52)]
    [Name("sole_trader_email")]
    public string SoleTraderEmail { get; set; }

    [Column(53)]
    [Name("approved_person_first_name")]
    public string ApprovedPersonFirstName { get; set; }

    [Column(54)]
    [Name("approved_person_last_name")]
    public string ApprovedPersonLastName { get; set; }

    [Column(55)]
    [Name("approved_person_phone_number")]
    public string ApprovedPersonPhoneNumber { get; set; }

    [Column(56)]
    [Name("approved_person_email")]
    public string ApprovedPersonEmail { get; set; }

    [Column(57)]
    [Name("approved_person_job_title")]
    public string ApprovedPersonJobTitle { get; set; }

    [Column(58)]
    [Name("delegated_person_first_name")]
    public string DelegatedPersonFirstName { get; set; }

    [Column(59)]
    [Name("delegated_person_last_name")]
    public string DelegatedPersonLastName { get; set; }

    [Column(60)]
    [Name("delegated_person_phone_number")]
    public string DelegatedPersonPhoneNumber { get; set; }

    [Column(61)]
    [Name("delegated_person_email")]
    public string DelegatedPersonEmail { get; set; }

    [Column(62)]
    [Name("delegated_person_job_title")]
    public string DelegatedPersonJobTitle { get; set; }

    [Column(63)]
    [Name("primary_contact_person_first_name")]
    public string PrimaryContactPersonFirstName { get; set; }

    [Column(64)]
    [Name("primary_contact_person_last_name")]
    public string PrimaryContactPersonLastName { get; set; }

    [Column(65)]
    [Name("primary_contact_person_phone_number")]
    public string PrimaryContactPersonPhoneNumber { get; set; }

    [Column(66)]
    [Name("primary_contact_person_email")]
    public string PrimaryContactPersonEmail { get; set; }

    [Column(67)]
    [Name("primary_contact_person_job_title")]
    public string PrimaryContactPersonJobTitle { get; set; }

    [Column(68)]
    [Name("secondary_contact_person_first_name")]
    public string SecondaryContactPersonFirstName { get; set; }

    [Column(69)]
    [Name("secondary_contact_person_last_name")]
    public string SecondaryContactPersonLastName { get; set; }

    [Column(70)]
    [Name("secondary_contact_person_phone_number")]
    public string SecondaryContactPersonPhoneNumber { get; set; }

    [Column(71)]
    [Name("secondary_contact_person_email")]
    public string SecondaryContactPersonEmail { get; set; }

    [Column(72)]
    [Name("secondary_contact_person_job_title")]
    public string SecondaryContactPersonJobTitle { get; set; }

    [Column(73)]
    [Name("organisation_size")]
    public string OrganisationSize { get; set; }

    [Column(74)]
    [Name(new string[] { "status_code", "leaver_code" })]
    public string LeaverCode { get; set; }

    [Column(75)]
    [Name("leaver_date")]
    public string LeaverDate { get; set; }

    [Column(76)]
    [Name("organisation_change_reason")]
    public string OrganisationChangeReason { get; set; }

    [Column(77)]
    [Name("joiner_date")]
    public string JoinerDate { get; set; }
}