﻿namespace EPR.RegistrationValidation.UnitTests.TestHelpers;

using Data.Models.QueueMessages;
using Newtonsoft.Json;

public static class QueueMessageTestHelper
{
    public static string GenerateMessage(
        string blobName,
        string submissionId,
        string submissionSubType,
        string userId,
        string organisationId,
        string complianceSchemeId,
        string userType)
    {
        var queueMessage = new BlobQueueMessage
        {
            BlobName = blobName,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType,
            UserId = userId,
            OrganisationId = organisationId,
            ComplianceSchemeId = complianceSchemeId,
            UserType = userType,
        };
        return JsonConvert.SerializeObject(queueMessage);
    }
}