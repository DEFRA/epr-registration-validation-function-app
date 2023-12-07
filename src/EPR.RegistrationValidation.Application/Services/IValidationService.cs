namespace EPR.RegistrationValidation.Application.Services;

using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.Data.Models.SubmissionApi;

public interface IValidationService
{
    Task<List<RegistrationValidationError>> ValidateAsync(IList<OrganisationDataRow> rows);
}