namespace EPR.RegistrationValidation.Application.Services;

using EPR.RegistrationValidation.Application.Clients;

public class ApiClients
{
    public ICompanyDetailsApiClient CompanyDetailsApiClient { get; set; }

    public ISubmissionApiClient SubmissionApiClient { get; set; }
}
