﻿namespace EPR.RegistrationValidation.UnitTests.TestHelpers;

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

    public static OrganisationDataRow GenerateParentOrganisation(string defraId)
    {
        var i = "999";

        return new OrganisationDataRow
        {
            DefraId = defraId,
            SubsidiaryId = string.Empty,
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
            ProduceBlankPackagingFlag = "Yes",
            Turnover = "9.99",
            ServiceOfNoticeAddressLine1 = "9 Surrey",
            ServiceOfNoticeAddressPostcode = "KT5 8JU",
            ServiceOfNoticeAddressPhoneNumber = "0123456789",
            AuditAddressLine1 = "10 Southcote",
            AuditAddressCountry = AuditingCountryCodes.England.ToLower(),
            AuditAddressPostcode = "KT5 9UW",
            TotalTonnage = "25",
            PrincipalAddressLine1 = "Principal Address Line 1",
            PrincipalAddressPostcode = "Principal Address Postcode",
            PrincipalAddressPhoneNumber = "01237946",
            OrganisationTypeCode = UnIncorporationTypeCodes.SoleTrader,
            OrganisationSize = OrganisationSizeCodes.L.ToString(),
            RegistrationTypeCode = RegistrationTypeCodes.Individual,
        };
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
                PackagingActivitySO = "Primary",
                PackagingActivityHl = "Secondary",
                PackagingActivityPf = "Secondary",
                PackagingActivitySl = "Secondary",
                PackagingActivityIm = "No",
                PackagingActivityOm = "No",
                PackagingActivitySe = "Secondary",
                ProduceBlankPackagingFlag = "Yes",
                Turnover = "9.99",
                ServiceOfNoticeAddressLine1 = "9 Surrey",
                ServiceOfNoticeAddressPostcode = "KT5 8JU",
                ServiceOfNoticeAddressPhoneNumber = "0123456789",
                AuditAddressLine1 = "10 Southcote",
                AuditAddressCountry = AuditingCountryCodes.England,
                AuditAddressPostcode = "KT5 9UW",
                TotalTonnage = "25",
                PrincipalAddressLine1 = "Principal Address Line 1",
                PrincipalAddressPostcode = "Principal Address Postcode",
                PrincipalAddressPhoneNumber = "01237946",
                OrganisationTypeCode = UnIncorporationTypeCodes.SoleTrader,
                OrganisationSize = OrganisationSizeCodes.L.ToString(),
                RegistrationTypeCode = RegistrationTypeCodes.Group,
            };
        }
    }

    public static IEnumerable<OrganisationDataRow> GenerateOrgIdSubId(int total)
    {
        var rows = new List<OrganisationDataRow>();

        for (int i = 0; i < total; i++)
        {
            var row = new OrganisationDataRow
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
                ProduceBlankPackagingFlag = "Yes",
                Turnover = "9.99",
                ServiceOfNoticeAddressLine1 = "9 Surrey",
                ServiceOfNoticeAddressPostcode = "KT5 8JU",
                ServiceOfNoticeAddressPhoneNumber = "0123456789",
                AuditAddressLine1 = "10 Southcote",
                AuditAddressCountry = AuditingCountryCodes.England.ToLower(),
                AuditAddressPostcode = "KT5 9UW",
                TotalTonnage = "25",
                PrincipalAddressLine1 = "Principal Address Line 1",
                PrincipalAddressPostcode = "Principal Address Postcode",
                PrincipalAddressPhoneNumber = "01237946",
                OrganisationTypeCode = UnIncorporationTypeCodes.SoleTrader,
                OrganisationSize = OrganisationSizeCodes.L.ToString(),
                JoinerDate = "01/01/2000",
                RegistrationTypeCode = RegistrationTypeCodes.Individual,
            };
            rows.Add(row);
            rows.Add(GenerateParentOrganisation(row.DefraId));
        }

        return rows;
    }

    public static IEnumerable<OrganisationDataRow> GenerateOrgIdSubIdWithoutParentOrg(int total)
    {
        var rows = new List<OrganisationDataRow>();

        for (int i = 0; i < total; i++)
        {
            var row = new OrganisationDataRow
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
                ProduceBlankPackagingFlag = "Yes",
                Turnover = "9.99",
                ServiceOfNoticeAddressLine1 = "9 Surrey",
                ServiceOfNoticeAddressPostcode = "KT5 8JU",
                ServiceOfNoticeAddressPhoneNumber = "0123456789",
                AuditAddressLine1 = "10 Southcote",
                AuditAddressCountry = AuditingCountryCodes.England.ToLower(),
                AuditAddressPostcode = "KT5 9UW",
                TotalTonnage = "25",
                PrincipalAddressLine1 = "Principal Address Line 1",
                PrincipalAddressPostcode = "Principal Address Postcode",
                PrincipalAddressPhoneNumber = "01237946",
                OrganisationTypeCode = UnIncorporationTypeCodes.SoleTrader,
                OrganisationSize = OrganisationSizeCodes.L.ToString(),
                JoinerDate = "01/01/2000",
            };
            rows.Add(row);
        }

        return rows;
    }

    public static IEnumerable<OrganisationDataRow> GenerateDuplicateOrgIdSubId(int total)
    {
        var rows = new List<OrganisationDataRow>();

        for (int i = 0; i < total; i++)
        {
            var row = new OrganisationDataRow
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
                Turnover = "9.99",
                ServiceOfNoticeAddressLine1 = "9 Surrey",
                ServiceOfNoticeAddressPostcode = "KT5 8JU",
                ServiceOfNoticeAddressPhoneNumber = "0123456789",
                TotalTonnage = "25",
                PrincipalAddressLine1 = "Principal Address Line 1",
                PrincipalAddressPostcode = "Principal Address Postcode",
                PrincipalAddressPhoneNumber = "01237946",
                OrganisationTypeCode = UnIncorporationTypeCodes.SoleTrader,
                OrganisationSize = OrganisationSizeCodes.L.ToString(),
                JoinerDate = "01/01/2000",
            };
            rows.Add(row);
            rows.Add(GenerateParentOrganisation(row.DefraId));
        }

        return rows;
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

    public static List<OrganisationDataRow> GenerateOrganisationWithExceededCharacterLimit(int columnIndex)
    {
        var organisationData = GenerateOrgs(1).First();
        switch (columnIndex)
        {
            case 0:
                organisationData.DefraId = new string('a', CharacterLimits.MaxLength + 1);
                break;
            case 1:
                organisationData.SubsidiaryId = new string('a', CharacterLimits.MaxLength + 1);
                break;
            case 2:
                organisationData.OrganisationName = new string('a', CharacterLimits.MaxLength + 1);
                break;
        }

        return new List<OrganisationDataRow> { organisationData };
    }

    public static IEnumerable<OrganisationDataRow> GenerateOrganisationCSVFile(int total)
    {
        for (int i = 0; i < total; i++)
        {
            yield return new OrganisationDataRow()
            {
                DefraId = $"{_randomizer.Next(100, 100 + total)}",
                SubsidiaryId = $"{_randomizer.Next(100, 100 + total)}",
            };
        }
    }

    public static IEnumerable<OrganisationDataRow> GenerateOrgs_WithoutOrganisationSizeField(int total)
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
                PackagingActivitySO = "Primary",
                PackagingActivityHl = "Secondary",
                PackagingActivityPf = "Secondary",
                PackagingActivitySl = "Secondary",
                PackagingActivityIm = "No",
                PackagingActivityOm = "No",
                PackagingActivitySe = "Secondary",
                ProduceBlankPackagingFlag = "Yes",
                Turnover = "9.99",
                ServiceOfNoticeAddressLine1 = "9 Surrey",
                ServiceOfNoticeAddressPostcode = "KT5 8JU",
                ServiceOfNoticeAddressPhoneNumber = "0123456789",
                AuditAddressLine1 = "10 Southcote",
                AuditAddressCountry = AuditingCountryCodes.England,
                AuditAddressPostcode = "KT5 9UW",
                TotalTonnage = "25",
                PrincipalAddressLine1 = "Principal Address Line 1",
                PrincipalAddressPostcode = "Principal Address Postcode",
                PrincipalAddressPhoneNumber = "01237946",
                OrganisationTypeCode = UnIncorporationTypeCodes.SoleTrader,
            };
        }
    }
}