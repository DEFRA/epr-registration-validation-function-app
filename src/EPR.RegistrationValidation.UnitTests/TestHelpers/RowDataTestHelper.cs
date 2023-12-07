namespace EPR.RegistrationValidation.UnitTests.TestHelpers;

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
                DefraId = $"{_randomizer.Next(100, 100 + total)}",
                OrganisationName = $"{i} ltd",
                HomeNationCode = "EN",
                PrimaryContactPersonFirstName = $"{i}FName",
                PrimaryContactPersonLastName = $"{i}LName",
                PrimaryContactPersonEmail = $"email{i}@test.com",
                PrimaryContactPersonPhoneNumber = $"07895462{i}",
            };
        }
    }
}