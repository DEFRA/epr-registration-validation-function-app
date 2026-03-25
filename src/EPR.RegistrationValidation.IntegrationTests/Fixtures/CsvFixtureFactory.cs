namespace EPR.RegistrationValidation.IntegrationTests.Fixtures;

public static class CsvFixtureFactory
{
    private const string OrganisationHeader = "organisation_id,subsidiary_id,organisation_name,trading_name,companies_house_number,home_nation_code,main_activity_sic,organisation_type_code,organisation_sub_type_code,packaging_activity_so,packaging_activity_pf,packaging_activity_im,packaging_activity_se,packaging_activity_hl,packaging_activity_om,packaging_activity_sl,registration_type_code,turnover,total_tonnage,produce_blank_packaging_flag,liable_for_disposal_costs_flag,meet_reporting_requirements_flag,registered_addr_line1,registered_addr_line2,registered_city,registered_addr_county,registered_addr_postcode,registered_addr_country,registered_addr_phone_number,audit_addr_line1,audit_addr_line2,audit_addr_city,audit_addr_county,audit_addr_postcode,audit_addr_country,service_of_notice_addr_line1,service_of_notice_addr_line2,service_of_notice_addr_city,service_of_notice_addr_county,service_of_notice_addr_postcode,service_of_notice_addr_country,service_of_notice_addr_phone_number,principal_addr_line1,principal_addr_line2,principal_addr_city,principal_addr_county,principal_addr_postcode,principal_addr_country,principal_addr_phone_number,sole_trader_first_name,sole_trader_last_name,sole_trader_phone_number,sole_trader_email,approved_person_first_name,approved_person_last_name,approved_person_phone_number,approved_person_email,approved_person_job_title,delegated_person_first_name,delegated_person_last_name,delegated_person_phone_number,delegated_person_email,delegated_person_job_title,primary_contact_person_first_name,primary_contact_person_last_name,primary_contact_person_phone_number,primary_contact_person_email,primary_contact_person_job_title,secondary_contact_person_first_name,secondary_contact_person_last_name,secondary_contact_person_phone_number,secondary_contact_person_email,secondary_contact_person_job_title,organisation_size,status_code,leaver_date,organisation_change_reason,joiner_date";
    private const string OrganisationHeaderWithClosedLoopRegistration = OrganisationHeader + ",closed_loop_registration";

    private const string ValidOrganisationRow = "145879,123456,Painting Ltd,,11893759,EN,20301,PAR,,Primary,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,01/01/2000";
    private const string SecondValidOrganisationRow = "213458,654321,Desk and Chairs,Furniture Store,8974610,EN,46150,LLP,,Secondary,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,01/01/2000";

    public static string ValidOrganisationCsv()
    {
        return string.Join(Environment.NewLine, OrganisationHeader, ValidOrganisationRow, SecondValidOrganisationRow);
    }

    public static string InvalidOrganisationHeaderCsv()
    {
        return string.Join(
            Environment.NewLine,
            OrganisationHeader.Replace("organisation_id", "organisaation_id", StringComparison.Ordinal),
            ValidOrganisationRow);
    }

    public static string OrganisationCsvWithMissingOrganisationId()
    {
        var rowWithMissingOrganisationId = "," + ValidOrganisationRow[(ValidOrganisationRow.IndexOf(',') + 1)..];
        return string.Join(Environment.NewLine, OrganisationHeader, rowWithMissingOrganisationId);
    }

    public static string OrganisationCsvWithClosedLoopRegistrationValue(string value)
    {
        return string.Join(
            Environment.NewLine,
            OrganisationHeaderWithClosedLoopRegistration,
            $"{ValidOrganisationRow},{value}");
    }

    public static string OrganisationCsvWithClosedLoopRegistrationEmptyValue()
    {
        return string.Join(
            Environment.NewLine,
            OrganisationHeaderWithClosedLoopRegistration,
            $"{ValidOrganisationRow},");
    }

    public static string BrandsCsv(string defraId, string subsidiaryId = "")
    {
        return string.Join(
            Environment.NewLine,
            "organisation_id,subsidiary_id,brand_name,brand_type_code",
            $"{defraId},{subsidiaryId},My Brand,OWN");
    }

    public static string PartnersCsv(string defraId, string subsidiaryId = "")
    {
        return string.Join(
            Environment.NewLine,
            "organisation_id,subsidiary_id,partner_first_name,partner_last_name,partner_phone_number,partner_email",
            $"{defraId},{subsidiaryId},John,Doe,01234567890,john.doe@example.com");
    }

    public static string OrganisationCsvForBrandCrossFileLookup(string defraId, string subsidiaryId)
    {
        var values = new string[78];
        values[0] = defraId;
        values[1] = subsidiaryId;
        values[2] = "Organisation Name";
        values[7] = "PAR";
        values[9] = "Primary";
        return string.Join(Environment.NewLine, OrganisationHeader, string.Join(",", values));
    }

    public static string OrganisationCsvForPartnerCrossFileLookup(string defraId, string subsidiaryId)
    {
        var values = new string[78];
        values[0] = defraId;
        values[1] = subsidiaryId;
        values[2] = "Organisation Name";
        values[7] = "PAR";
        return string.Join(Environment.NewLine, OrganisationHeader, string.Join(",", values));
    }
}
