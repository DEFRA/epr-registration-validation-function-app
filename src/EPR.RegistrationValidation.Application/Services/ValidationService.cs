namespace EPR.RegistrationValidation.Application.Services;

using EPR.RegistrationValidation.Application.Helpers;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Config;
using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.Data.Models.SubmissionApi;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ValidationService : IValidationService
{
    private readonly OrganisationDataRowValidator _rowValidator;
    private readonly ColumnMetaDataProvider _metaDataProvider;
    private readonly ValidationSettings _validationSettings;
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(
        OrganisationDataRowValidator rowValidator,
        ColumnMetaDataProvider metaDataProvider,
        IOptions<ValidationSettings> validationSettings,
        ILogger<ValidationService> logger)
    {
        _rowValidator = rowValidator;
        _metaDataProvider = metaDataProvider;
        _logger = logger;
        _validationSettings = validationSettings.Value;
    }

    public async Task<List<RegistrationValidationError>> ValidateAsync(IList<OrganisationDataRow> rows)
    {
        List<RegistrationValidationError> validationErrors = new();
        int totalErrors = 0;

        for (int i = 0; i < rows.Count && totalErrors < _validationSettings.ErrorLimit; i++)
        {
            var row = rows[i];
            var result = await _rowValidator.ValidateAsync(row);

            if (result.IsValid)
            {
                _logger.LogInformation("Row {Row} validated successfully", i);
                continue;
            }

            var error = new RegistrationValidationError
            {
                RowNumber = i,
                OrganisationId = row.DefraId,
                SubsidiaryId = row.SubsidiaryId,
            };

            foreach (var validationError in result.Errors.TakeWhile(_ => totalErrors < _validationSettings.ErrorLimit))
            {
                var columnMeta = _metaDataProvider.GetOrganisationColumnMetaData(validationError.PropertyName);
                error.ColumnErrors.Add(new ColumnValidationError
                {
                    ErrorCode = validationError.ErrorCode,
                    ColumnIndex = columnMeta?.Index ?? -1,
                    ColumnName = columnMeta?.Name,
                });

                LogValidationWarning(i, validationError);
                totalErrors++;
            }

            validationErrors.Add(error);
        }

        _logger.LogInformation("Total validation errors {Count}", validationErrors.Count);
        return validationErrors;
    }

    private void LogValidationWarning(int row, ValidationFailure validationError)
    {
        _logger.LogWarning(
            "Validation error on row {Row} {ErrorMessage} Error code {ErrorCode}",
            row,
            validationError.ErrorMessage,
            validationError.ErrorCode);
    }
}