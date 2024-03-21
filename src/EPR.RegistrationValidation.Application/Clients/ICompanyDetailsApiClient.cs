namespace EPR.RegistrationValidation.Application.Clients;

using Data.Models.CompanyDetailsApi;

public interface ICompanyDetailsApiClient
{
    Task<CompanyDetailsDataResult> GetCompanyDetails(string organisationId);

    Task<CompanyDetailsDataResult> GetComplianceSchemeMembers(string organisationId, string complianceSchemeId);

    Task<CompanyDetailsDataResult> GetRemainingProducerDetails(IEnumerable<string> referenceNumbers);
}