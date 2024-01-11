namespace EPR.RegistrationValidation.Data.Models.SubmissionApi;

public class RegistrationValidationEvent : ValidationEvent
{
    public List<RegistrationValidationError>? ValidationErrors { get; init; }

    public bool RequiresBrandsFile { get; init; }

    public bool RequiresPartnershipsFile { get; init; }
}