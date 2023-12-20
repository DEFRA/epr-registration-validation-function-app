namespace EPR.RegistrationValidation.UnitTests.TestHelpers;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Models;

public static class RowDataTestHelper
{
    private static readonly Random _randomizer = new();

    public static IEnumerable<OrganisationDataRow> GenerateInvalidOrgs(int total)
    {
        for (int i = 0; i < total; i++)
        {
            yield return new OrganisationDataRow { DefraId = null };
        }
    }

    public static IEnumerable<OrganisationDataRow> GenerateOrgs(int total)
    {
        for (int i = 0; i < total; i++)
        {
            yield return new OrganisationDataRow
            {
                DefraId = "12345" + i,
                OrganisationName = $"{i} ltd",
                TradingName = $"{i} trading name",
                HomeNationCode = "EN",
                MainActivitySic = "99999", // Dormant Company
                PrimaryContactPersonFirstName = $"{i}FName",
                PrimaryContactPersonLastName = $"{i}LName",
                PrimaryContactPersonEmail = $"email{i}@test.com",
                PrimaryContactPersonPhoneNumber = $"07895462{i}",
                AuditAddressCountry = AuditingCountryCodes.England,
                PackagingActivitySO = "Primary",
                PackagingActivityHl = "Secondary",
                PackagingActivityPf = "Secondary",
                PackagingActivitySl = "Secondary",
                PackagingActivityIm = "No",
                PackagingActivityOm = "No",
                PackagingActivitySe = "Secondary",
            };
        }
    }

    public static IEnumerable<OrganisationDataRow> GenerateOrgIdSubId(int total)
    {
        for (int i = 0; i < total; i++)
        {
            yield return new OrganisationDataRow
            {
                DefraId = "12345" + i,
                SubsidiaryId = "678",
                OrganisationName = $"{i} ltd",
                HomeNationCode = "EN",
                PrimaryContactPersonFirstName = $"{i}FName",
                PrimaryContactPersonLastName = $"{i}LName",
                PrimaryContactPersonEmail = $"email{i}@test.com",
                PrimaryContactPersonPhoneNumber = $"07895462{i}",
                PackagingActivitySO = "Primary",
                PackagingActivityHl = "Secondary",
                PackagingActivityPf = "Secondary",
                PackagingActivitySl = "Secondary",
                PackagingActivityIm = "No",
                PackagingActivityOm = "No",
                PackagingActivitySe = "Secondary",
            };
        }
    }

    public static IEnumerable<OrganisationDataRow> GenerateDuplicateOrgIdSubId(int total)
    {
        for (int i = 0; i < total; i++)
        {
            yield return new OrganisationDataRow
            {
                DefraId = "12345",
                SubsidiaryId = "678", OrganisationName = $"{i} ltd",
                HomeNationCode = "EN",
                PrimaryContactPersonFirstName = $"{i}FName",
                PrimaryContactPersonLastName = $"{i}LName",
                PrimaryContactPersonEmail = $"email{i}@test.com",
                PrimaryContactPersonPhoneNumber = $"07895462{i}",
                PackagingActivitySO = "Primary",
                PackagingActivityHl = "Secondary",
                PackagingActivityPf = "Secondary",
                PackagingActivitySl = "Secondary",
                PackagingActivityIm = "No",
                PackagingActivityOm = "No",
                PackagingActivitySe = "Secondary",
            };
        }
    }
}