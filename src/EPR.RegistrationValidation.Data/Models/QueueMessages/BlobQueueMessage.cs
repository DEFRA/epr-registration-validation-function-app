namespace EPR.RegistrationValidation.Data.Models.QueueMessages;

using System.ComponentModel.DataAnnotations;

public class BlobQueueMessage
{
    [Required]
    public string BlobName { get; init; }

    public string SubmissionId { get; init; }

    public string SubmissionSubType { get; init; }

    public string UserType { get; init; }

    public string UserId { get; init; }

    public string OrganisationId { get; init; }

    public bool? RequiresRowValidation { get; init; }
}