namespace EPR.RegistrationValidation.Data.Models.Subsidiary
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class SubsidiaryDetail
    {
        public string ReferenceNumber { get; set; }

        public string CompaniesHouseNumber { get; set; }

        public bool SubsidiaryExists { get; set; }

        public bool SubsidiaryBelongsToAnyOtherOrganisation { get; set; }

        public bool SubsidiaryDoesNotBelongToAnyOrganisation { get; set; }

        public DateTime? JoinerDate { get; set; }

        public string ReportingType { get; set; }
    }
}
