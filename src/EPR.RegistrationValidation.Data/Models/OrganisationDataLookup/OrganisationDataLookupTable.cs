namespace EPR.RegistrationValidation.Data.Models.OrganisationDataLookup;

public record OrganisationDataLookupTable(
    Dictionary<string, Dictionary<string, OrganisationIdentifiers>> Data);
