namespace EPR.RegistrationValidation.Data.Models.Services
{
    public class ValidateCompanyDetailsModel
    {
        public List<OrganisationDataRow> OrganisationDataRows { get; set; }

        public int TotalErrors { get; set; }

        public string ComplianceSchemeId { get; set; }

        public string UserId { get; set; }

        public string ProducerOrganisationId { get; set; }
    }
}
