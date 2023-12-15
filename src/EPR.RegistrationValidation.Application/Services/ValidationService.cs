namespace EPR.RegistrationValidation.Application.Services;

using EPR.RegistrationValidation.Application.Helpers;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Config;
using EPR.RegistrationValidation.Data.Constants;
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

        var rowValidationResult = await ValidateRowsAsync(rows);
        validationErrors.AddRange(rowValidationResult.ValidationErrors);

        var duplicateValidationResult = ValidateDuplicates(rows, rowValidationResult.TotalErrors);
        validationErrors.AddRange(duplicateValidationResult.ValidationErrors);

        _logger.LogInformation("Total validation errors {Count}", duplicateValidationResult.TotalErrors);
        return validationErrors;
    }

    public async Task<(int TotalErrors, List<RegistrationValidationError> ValidationErrors)> ValidateRowsAsync(IList<OrganisationDataRow> rows)
    {
        List<RegistrationValidationError> validationErrors = new();
        int totalErrors = 0;
        foreach (var row in rows.TakeWhile(_ => totalErrors < _validationSettings.ErrorLimit))
        {
            var result = await _rowValidator.ValidateAsync(row);

            if (result.IsValid)
            {
                _logger.LogInformation("Row {Row} validated successfully", row.LineNumber);
                continue;
            }

            var error = new RegistrationValidationError
            {
                RowNumber = row.LineNumber,
                OrganisationId = row.DefraId,
                SubsidiaryId = row.SubsidiaryId,
            };

            foreach (var validationError in result.Errors.TakeWhile(_ => totalErrors < _validationSettings.ErrorLimit))
            {
                var columnMeta = _metaDataProvider.GetOrganisationColumnMetaData(validationError.PropertyName);
                error.ColumnErrors.Add(new ColumnValidationError
                {
                    ErrorCode = validationError.ErrorCode,
                    ColumnIndex = columnMeta?.Index,
                    ColumnName = columnMeta?.Name,
                });

                LogValidationWarning(row.LineNumber, validationError);
                totalErrors++;
            }

            validationErrors.Add(error);
        }

        return (totalErrors, validationErrors);
    }

    public (int TotalErrors, List<RegistrationValidationError> ValidationErrors) ValidateDuplicates(IList<OrganisationDataRow> rows, int totalErrors)
    {
        List<RegistrationValidationError> validationErrors = new();

        var duplicateRows = rows
            .GroupBy(row => (row?.DefraId, row?.SubsidiaryId))
            .Where(row => row.Count() > 1);

        var defraIdColumn = _metaDataProvider.GetOrganisationColumnMetaData(nameof(OrganisationDataRow.DefraId));

        var defraIdValidationError = new ColumnValidationError
        {
            ErrorCode = ErrorCodes.DuplicateOrganisationIdSubsidiaryId,
            ColumnIndex = defraIdColumn?.Index,
            ColumnName = defraIdColumn?.Name,
        };

        foreach (var row in duplicateRows
                     .SelectMany(x => x)
                     .TakeWhile(_ => totalErrors < _validationSettings.ErrorLimit))
        {
            var error = new RegistrationValidationError
            {
                RowNumber = row.LineNumber,
                OrganisationId = row.DefraId,
                SubsidiaryId = row.SubsidiaryId,
            };
            error.ColumnErrors.Add(defraIdValidationError);

            var errorMessage = $"Duplicate record found for organisation id = {row.DefraId} subsidiary id = {row.SubsidiaryId}";

            LogValidationWarning(row.LineNumber, errorMessage, ErrorCodes.DuplicateOrganisationIdSubsidiaryId);
            validationErrors.Add(error);
            totalErrors++;
        }

        return (totalErrors, validationErrors);
    }

    private void LogValidationWarning(int row, ValidationFailure validationError)
    {
        LogValidationWarning(row, validationError.ErrorMessage, validationError.ErrorCode);
    }

    private void LogValidationWarning(int row, string errorMessage, string errorCode)
    {
        _logger.LogWarning(
            "Validation error on row {Row} {ErrorMessage} Error code {ErrorCode}",
            row,
            errorMessage,
            errorCode);
    }
}