namespace EPR.RegistrationValidation.Application.Clients;

using Data.Models.CompanyDetailsApi;
using Data.Models.Subsidiary;

public interface ICompanyDetailsApiClient
{
    Task<CompanyDetailsDataResult> GetCompanyDetails(string organisationId);

    Task<CompanyDetailsDataResult> GetCompanyDetailsByProducer(string producerOrganisationId);

    Task<CompanyDetailsDataResult> GetComplianceSchemeMembers(string organisationId, string complianceSchemeId);

    Task<CompanyDetailsDataResult> GetRemainingProducerDetails(IEnumerable<string> referenceNumbers);

    Task<SubsidiaryDetailsResponse> GetSubsidiaryDetails(SubsidiaryDetailsRequest subsidiaryDetailsRequest);
}