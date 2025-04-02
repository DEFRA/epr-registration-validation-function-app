namespace EPR.RegistrationValidation.Application.Services;

using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.Data.Models.OrganisationDataLookup;
using EPR.RegistrationValidation.Data.Models.QueueMessages;
using EPR.RegistrationValidation.Data.Models.SubmissionApi;

public interface IValidationService
{
    Task<List<RegistrationValidationError>> ValidateOrganisationsAsync(List<OrganisationDataRow> rows, BlobQueueMessage blobQueueMessage, bool validateCompanyDetailsData);

    Task<List<RegistrationValidationWarning>> ValidateOrganisationWarningsAsync(List<OrganisationDataRow> rows);

    bool IsColumnLengthExceeded(List<OrganisationDataRow> rows);

    Task<List<string>> ValidateAppendedFileAsync<T>(List<T> rows, OrganisationDataLookupTable organisationLookup)
        where T : ICsvDataRow;
}