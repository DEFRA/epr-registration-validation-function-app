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

    public static IEnumerable<BrandDataRow> GenerateBrand(int total)
    {
        for (int i = 0; i < total; i++)
        {
            yield return new BrandDataRow()
            {
                DefraId = $"{_randomizer.Next(100, 100 + total)}",
                SubsidiaryId = $"{_randomizer.Next(100, 100 + total)}",
                BrandName = $"{i}BrandName",
                BrandTypeCode = $"{i}BrandTypeCode",
            };
        }
    }

    public static List<BrandDataRow> GenerateBrandWithExceededCharacterLimit(int columnIndex)
    {
        var brandData = GenerateBrand(1).First();
        switch (columnIndex)
        {
            case 0:
                brandData.DefraId = new string('a', CharacterLimits.MaxLength + 1);
                break;
            case 1:
                brandData.SubsidiaryId = new string('a', CharacterLimits.MaxLength + 1);
                break;
            case 2:
                brandData.BrandName = new string('a', CharacterLimits.MaxLength + 1);
                break;
            case 3:
                brandData.BrandTypeCode = new string('a', CharacterLimits.MaxLength + 1);
                break;
        }

        return new List<BrandDataRow> { brandData };
    }

    public static IEnumerable<PartnersDataRow> GeneratePartner(int total)
    {
        for (int i = 0; i < total; i++)
        {
            yield return new PartnersDataRow()
            {
                DefraId = $"{_randomizer.Next(100, 100 + total)}",
                SubsidiaryId = $"{_randomizer.Next(100, 100 + total)}",
                PartnerFirstName = $"{i}PartnerFirstName",
                PartnerLastName = $"{i}PartnerLastName",
                PartnerPhoneNumber = $"{i}PartnerPhoneNumber",
                PartnerEmail = $"{i}PartnerEmail",
            };
        }
    }

    public static List<PartnersDataRow> GeneratePartnerWithExceededCharacterLimit(int columnIndex)
    {
        var partnerData = GeneratePartner(1).First();
        switch (columnIndex)
        {
            case 0:
                partnerData.DefraId = new string('a', CharacterLimits.MaxLength + 1);
                break;
            case 1:
                partnerData.SubsidiaryId = new string('a', CharacterLimits.MaxLength + 1);
                break;
            case 2:
                partnerData.PartnerFirstName = new string('a', CharacterLimits.MaxLength + 1);
                break;
            case 3:
                partnerData.PartnerLastName = new string('a', CharacterLimits.MaxLength + 1);
                break;
            case 5:
                partnerData.PartnerPhoneNumber = new string('a', CharacterLimits.MaxLength + 1);
                break;
            case 6:
                partnerData.PartnerEmail = new string('a', CharacterLimits.MaxLength + 1);
                break;
        }

        return new List<PartnersDataRow> { partnerData };
    }
}