namespace EPR.RegistrationValidation.Data.Models.CompanyDetailsApi;

public class OrganisationReferences
{
    public IEnumerable<string> ReferenceNumbers { get; set; }

    public string OrganisationExternalId { get; set; }
}