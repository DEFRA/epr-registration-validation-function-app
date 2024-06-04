namespace EPR.RegistrationValidation.UnitTests.TestHelpers;

using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.Data.Models.OrganisationDataLookup;

public static class OrganisationDataLookupTableTestHelper
{
    public static OrganisationDataLookupTable GenerateEmptyTable()
    {
        return new OrganisationDataLookupTable(null);
    }

    public static OrganisationDataLookupTable GenerateWithInvalidData()
    {
        return new OrganisationDataLookupTable(
            new Dictionary<string, Dictionary<string, OrganisationIdentifiers>>
            {
                {
                    "999999", new()
                    {
                        { "888888", new OrganisationIdentifiers("999999", "888888") },
                    }
                },
            });
    }

    public static OrganisationDataLookupTable GenerateWithInvalidOrganisationId()
    {
        return new OrganisationDataLookupTable(
            new Dictionary<string, Dictionary<string, OrganisationIdentifiers>>
            {
                {
                    "999999", new()
                    {
                        { "888888", new OrganisationIdentifiers("999999", "888888") },
                    }
                },
            });
    }

    public static OrganisationDataLookupTable GenerateWithInvalidSubsidiaryId()
    {
        return new OrganisationDataLookupTable(
            new Dictionary<string, Dictionary<string, OrganisationIdentifiers>>
            {
                {
                    "999999", new()
                    {
                        { "888888", new OrganisationIdentifiers("999999", "888888") },
                    }
                },
            });
    }

    public static OrganisationDataLookupTable GenerateWithValues(string defraId, string subsidiaryId)
    {
        return new OrganisationDataLookupTable(
            new Dictionary<string, Dictionary<string, OrganisationIdentifiers>>
            {
                {
                    defraId, new()
                    {
                        { subsidiaryId, new OrganisationIdentifiers(defraId, subsidiaryId) },
                    }
                },
            });
    }

    public static OrganisationDataLookupTable GenerateFromCsvRows<T>(IEnumerable<T> dataRows)
    {
        return typeof(T).Name switch
        {
            nameof(BrandDataRow) =>
                new OrganisationDataLookupTable(
                    dataRows
                    .Cast<BrandDataRow>()
                    .GroupBy(o => o.DefraId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.DistinctBy(d => d.SubsidiaryId)
                              .ToDictionary(
                                  i => i.SubsidiaryId ?? string.Empty,
                                  i => new OrganisationIdentifiers(i.DefraId, i.SubsidiaryId)))),
            nameof(PartnersDataRow) =>
                new OrganisationDataLookupTable(
                    dataRows
                    .Cast<PartnersDataRow>()
                    .GroupBy(o => o.DefraId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.DistinctBy(d => d.SubsidiaryId)
                              .ToDictionary(
                                  i => i.SubsidiaryId ?? string.Empty,
                                  i => new OrganisationIdentifiers(i.DefraId, i.SubsidiaryId)))),
        };
    }
}