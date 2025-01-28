namespace EPR.RegistrationValidation.Data.Models.Subsidiary
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class SubsidiaryOrganisationDetail
    {
        public string OrganisationReference { get; set; }

        public List<SubsidiaryDetail> SubsidiaryDetails { get; set; }
    }
}
