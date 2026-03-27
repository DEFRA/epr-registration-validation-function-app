namespace EPR.RegistrationValidation.IntegrationTests.Fakes;

using EPR.RegistrationValidation.Application.Clients;
using EPR.RegistrationValidation.Data.Models.SubmissionApi;

public class InMemorySubmissionApiClient : ISubmissionApiClient
{
    public ValidationEvent CapturedValidationEvent { get; private set; }

    public string CapturedOrganisationId { get; private set; }

    public string CapturedUserId { get; private set; }

    public string CapturedSubmissionId { get; private set; }

    public string CapturedUserType { get; private set; }

    public OrganisationFileDetailsResponse OrganisationFileDetailsResponse { get; set; } = new()
    {
        BlobName = "organisation-file.csv",
        SubmissionPeriod = "January to June 2025",
        RegistrationJourney = "DATA-PROVIDER",
    };

    public Task<OrganisationFileDetailsResponse> GetOrganisationFileDetails(string submissionId, string brandOrPartnerBlobName)
    {
        return Task.FromResult(OrganisationFileDetailsResponse);
    }

    public Task SendEventRegistrationMessage(string orgId, string userId, string submissionId, string userType, ValidationEvent validationEvent)
    {
        CapturedOrganisationId = orgId;
        CapturedUserId = userId;
        CapturedSubmissionId = submissionId;
        CapturedUserType = userType;
        CapturedValidationEvent = validationEvent;
        return Task.CompletedTask;
    }
}
