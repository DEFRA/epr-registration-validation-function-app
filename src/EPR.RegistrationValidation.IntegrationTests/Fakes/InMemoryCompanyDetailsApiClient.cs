namespace EPR.RegistrationValidation.IntegrationTests.Fakes;

using EPR.RegistrationValidation.Application.Clients;
using EPR.RegistrationValidation.Data.Models.CompanyDetailsApi;
using EPR.RegistrationValidation.Data.Models.Subsidiary;

public class InMemoryCompanyDetailsApiClient : ICompanyDetailsApiClient
{
    public CompanyDetailsDataResult ProducerDetailsResult { get; set; } = new()
    {
        Organisations = new List<CompanyDetailsDataItem>(),
    };

    public CompanyDetailsDataResult ComplianceSchemeMembersResult { get; set; } = new()
    {
        Organisations = new List<CompanyDetailsDataItem>(),
    };

    public CompanyDetailsDataResult RemainingProducerDetailsResult { get; set; } = new()
    {
        Organisations = new List<CompanyDetailsDataItem>(),
    };

    public SubsidiaryDetailsResponse SubsidiaryDetailsResult { get; set; } = new()
    {
        SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>(),
    };

    public Task<CompanyDetailsDataResult> GetCompanyDetails(string organisationId)
    {
        return Task.FromResult(ProducerDetailsResult);
    }

    public Task<CompanyDetailsDataResult> GetCompanyDetailsByProducer(string producerOrganisationId)
    {
        return Task.FromResult(ProducerDetailsResult);
    }

    public Task<CompanyDetailsDataResult> GetComplianceSchemeMembers(string organisationId, string complianceSchemeId)
    {
        return Task.FromResult(ComplianceSchemeMembersResult);
    }

    public Task<CompanyDetailsDataResult> GetRemainingProducerDetails(IEnumerable<string> referenceNumbers)
    {
        return Task.FromResult(RemainingProducerDetailsResult);
    }

    public Task<SubsidiaryDetailsResponse> GetSubsidiaryDetails(SubsidiaryDetailsRequest subsidiaryDetailsRequest)
    {
        return Task.FromResult(SubsidiaryDetailsResult);
    }
}
