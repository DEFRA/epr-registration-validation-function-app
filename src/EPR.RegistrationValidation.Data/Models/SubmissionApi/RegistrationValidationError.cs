namespace EPR.RegistrationValidation.Data.Models.SubmissionApi;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class RegistrationValidationError
{
    public int RowNumber { get; set; }

    public List<string> ErrorCode { get; set; }
}