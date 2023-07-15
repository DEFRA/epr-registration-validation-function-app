namespace EPR.RegistrationValidation.Data.Models.SubmissionApi;

using Enums;

public class RegistrationEvent
{
    public EventType Type { get; init; }

    public List<string>? Errors { get; init; }

    public List<RegistrationValidationError>? ValidationErrors { get; init; }

    public bool IsValid { get; init; }

    public bool RequiresBrandsFile { get; init; }

    public bool RequiresPartnershipsFile { get; init; }

    public string BlobName { get; set; }
}