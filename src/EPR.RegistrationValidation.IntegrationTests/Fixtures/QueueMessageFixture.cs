namespace EPR.RegistrationValidation.IntegrationTests.Fixtures;

using EPR.RegistrationValidation.Data.Models.QueueMessages;
using Newtonsoft.Json;

public static class QueueMessageFixture
{
    public static string Build(
        string blobName,
        string submissionSubType,
        bool requiresRowValidation = true,
        string submissionId = "submission-id",
        string organisationId = "organisation-id",
        string userId = "user-id",
        string userType = "Producer",
        string complianceSchemeId = null)
    {
        var queueMessage = new BlobQueueMessage
        {
            BlobName = blobName,
            SubmissionSubType = submissionSubType,
            SubmissionId = submissionId,
            OrganisationId = organisationId,
            UserId = userId,
            UserType = userType,
            ComplianceSchemeId = complianceSchemeId,
            RequiresRowValidation = requiresRowValidation,
        };

        return JsonConvert.SerializeObject(queueMessage);
    }
}
