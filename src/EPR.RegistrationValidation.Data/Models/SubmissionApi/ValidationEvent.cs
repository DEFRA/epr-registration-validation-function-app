namespace EPR.RegistrationValidation.Data.Models.SubmissionApi;

using Enums;

public class ValidationEvent
{
    public EventType Type { get; init; }

    public List<string>? Errors { get; init; }

    public bool IsValid { get; init; }

    public string BlobName { get; set; }

    public string BlobContainerName { get; set; }

    public int? OrganisationMemberCount { get; set; }
}