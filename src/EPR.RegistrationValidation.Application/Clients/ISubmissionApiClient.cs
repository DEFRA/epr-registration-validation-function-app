namespace EPR.RegistrationValidation.Application.Clients;

using Data.Models.SubmissionApi;

public interface ISubmissionApiClient
{
    Task<OrganisationFileDetailsResponse> GetOrganisationFileDetails(string submissionId, string brandOrPartnerBlobName);

    Task SendEventRegistrationMessage(string orgId, string userId, string submissionId, string userType, ValidationEvent validationEvent);
}