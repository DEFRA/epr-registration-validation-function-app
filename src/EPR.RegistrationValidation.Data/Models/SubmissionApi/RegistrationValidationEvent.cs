namespace EPR.RegistrationValidation.Data.Models.SubmissionApi;

public class RegistrationValidationEvent : ValidationEvent
{
    public List<RegistrationValidationError>? ValidationErrors { get; init; }

    public List<RegistrationValidationWarning>? ValidationWarnings { get; init; }

    public bool RequiresBrandsFile { get; init; }

    public bool RequiresPartnershipsFile { get; init; }

    public bool? HasMaxRowErrors { get; set; }

    public bool? HasMaxRowWarnings { get; set; }

    public int? RowErrorCount => ValidationErrors?.SelectMany(x => x.ColumnErrors).Count();

    public int? RowWarningCount => ValidationWarnings?.SelectMany(x => x.ColumnErrors).Count();
}