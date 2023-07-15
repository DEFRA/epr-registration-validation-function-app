namespace EPR.RegistrationValidation.UnitTests.SampleData;

using Data.Models;

public class SampleCompanyData
{
    public const string Header =
        @"Defra Id,Subsidiary Id,organisation_name,trading_name,companies_house_number,home_nation_code,main_activity_sic,organisation_type_code,organisation_sub_type_code,producer_type_code,organisation_status_type_code,packaging_activity_so,packaging_activity_pf,packaging_activity_im,packaging_activity_se,packaging_activity_hl,packaging_activity_om,packaging_activity_sl,registration_type_code,turnover,main_activity_tonnage,total_tonnage,produce_blank_packaging_flag,liable_for_disposal_costs_flag,meet_reporting_requirements_flag,registered_addr_line1,registered_addr_line2,registered_addr_line3,registered_addr_line4,registered_city,registered_addr_county,registered_addr_country,registered_addr_postcode,registered_addr_phone_number,audit_addr_line1,audit_addr_line2,audit_addr_line3,audit_addr_line4,audit_addr_city,audit_addr_county,audit_addr_country,audit_addr_postcode,audit_addr_line1,audit_addr_line2,audit_addr_line3,audit_addr_line4,audit_addr_city,audit_addr_county,audit_addr_country,audit_addr_postcode,correspondence_addr_line1,correspondence_addr_line2,correspondence_addr_line3,correspondence_addr_line4,correspondence_addr_city,correspondence_addr_county,correspondence_addr_country,correspondence_addr_postcode,principal_addr_line1,principal_addr_line2,principal_addr_line3,principal_addr_line4,principal_addr_city,principal_addr_county,principal_addr_country,principal_addr_postcode,principal_addr_phone_number,sole_trader_first_name,sole_trader_last_name,sole_trader_phone_number,sole_trader_email,approved_person_first_name,approved_person_last_name,approved_person_phone_number,approved_person_email,approved_person_job_title,delegated_person_first_name,delegated_person_last_name,delegated_person_phone_number,delegated_person_email,delegated_person_job_title,primary_contact_person_first_name,primary_contact_person_last_name,primary_contact_person_phone_number,primary_contact_person_email,primary_contact_person_job_title,secondary_contact_person_first_name,secondary_contact_person_last_name,secondary_contact_person_phone_number,secondary_contact_person_email,secondary_contact_person_job_title";

    public static string GenerateDummyFileString(List<CsvDataRow> csvDataRows)
    {
        string stringFile = Header + Environment.NewLine;
        foreach (var row in csvDataRows)
        {
            string rowString = string.Format(
                "{0},{1},Painting Ltd,,11893759,EN,20301,{2},,NO,AC,{3},N,N,N,N,N,N,IN,2647683,BO,60,Y,Y,Y,52 Castle Meadow,,,,Norwich,Norfolk,England,NR1 3DD,01603 611200,,,,,,,,,,,,,,,,,7 Elmfield Rd,,,,Newcastle upon Tyne,Tyne and Wear,England,NE3 4AY,,,,,,,,,,,,,,Raynor,Dorcey,01603 611200,r.dorcey@paintingltd.co.uk,Director,Quentin,Hewitt,7067279189,q.hewitt@paintingltd.co.uk,Manager,Kevin,Malloney,01603 611200,k.Malloney@paintingltd.co.uk,,,,,,",
                row.DefraId,
                row.SubsidiaryId,
                row.OrganisationTypeCode,
                row.PackagingActivitySO);
            stringFile += rowString + Environment.NewLine;
        }

        return stringFile;
    }
}