namespace EPR.RegistrationValidation.Data.Models.SubmissionApi;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class RegistrationValidationError
{
    public int RowNumber { get; set; }

    public string OrganisationId { get; set; }

    public string SubsidiaryId { get; set; }

    public ICollection<ColumnValidationError> ColumnErrors { get; set; } = new List<ColumnValidationError>();
}