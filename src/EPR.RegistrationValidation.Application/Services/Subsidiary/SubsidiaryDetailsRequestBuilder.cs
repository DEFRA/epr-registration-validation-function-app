namespace EPR.RegistrationValidation.Application.Services.Subsidiary
{
    using EPR.RegistrationValidation.Data.Models;
    using EPR.RegistrationValidation.Data.Models.Subsidiary;

    public class SubsidiaryDetailsRequestBuilder : ISubsidiaryDetailsRequestBuilder
    {
        public SubsidiaryDetailsRequest CreateRequest(List<OrganisationDataRow> rows)
        {
            var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest
            {
                SubsidiaryOrganisationDetails = rows
             .GroupBy(row => row.DefraId)
             .Where(group => group.Any(row => !string.IsNullOrEmpty(row.SubsidiaryId)))
             .Select(group => new SubsidiaryOrganisationDetail
             {
                 OrganisationReference = group.Key,
                 SubsidiaryDetails = group
                     .Where(row => !string.IsNullOrEmpty(row.SubsidiaryId))
                     .Select(row => new SubsidiaryDetail
                     {
                         ReferenceNumber = row.SubsidiaryId,
                     }).ToList(),
             })
             .ToList(),
            };

            return subsidiaryDetailsRequest;
        }
    }
}
