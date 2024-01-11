namespace EPR.RegistrationValidation.Application.Clients;

using Data.Models.SubmissionApi;

public interface ISubmissionApiClient
{
    Task SendEventRegistrationMessage(string orgId, string userId, string submissionId, string userType, ValidationEvent validationEvent);
}