﻿namespace EPR.RegistrationValidation.Application.Services;

using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.Data.Models.SubmissionApi;

public interface IValidationService
{
    Task<List<RegistrationValidationError>> ValidateOrganisationsAsync(List<OrganisationDataRow> rows);

    bool IsColumnLengthExceeded(List<OrganisationDataRow> rows);

    Task<List<string>> ValidateAppendedFileAsync<T>(List<T> rows)
        where T : ICsvDataRow;
}