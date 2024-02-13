namespace EPR.RegistrationValidation.Application.Services;

using System.Reflection;
using CsvHelper.Configuration.Attributes;
using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Helpers;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Attributes;
using EPR.RegistrationValidation.Data.Config;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.Data.Models.SubmissionApi;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ValidationService : IValidationService
{
    private readonly OrganisationDataRowValidator _organisationDataRowValidator;
    private readonly BrandDataRowValidator _brandDataRowValidator;
    private readonly PartnerDataRowValidator _partnerDataRowValidator;
    private readonly ColumnMetaDataProvider _metaDataProvider;
    private readonly ValidationSettings _validationSettings;
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(
        OrganisationDataRowValidator organisationDataRowValidator,
        BrandDataRowValidator brandDataRowValidator,
        PartnerDataRowValidator partnerDataRowValidator,
        ColumnMetaDataProvider metaDataProvider,
        IOptions<ValidationSettings> validationSettings,
        ILogger<ValidationService> logger)
    {
        _organisationDataRowValidator = organisationDataRowValidator;
        _brandDataRowValidator = brandDataRowValidator;
        _partnerDataRowValidator = partnerDataRowValidator;
        _metaDataProvider = metaDataProvider;
        _logger = logger;
        _validationSettings = validationSettings.Value;
    }

    public async Task<List<RegistrationValidationError>> ValidateOrganisationsAsync(List<OrganisationDataRow> rows)
    {
        List<RegistrationValidationError> validationErrors = new();

        var rowValidationResult = await ValidateRowsAsync(rows);
        validationErrors.AddRange(rowValidationResult.ValidationErrors);

        var duplicateValidationResult = ValidateDuplicates(rows, rowValidationResult.TotalErrors);
        validationErrors.AddRange(duplicateValidationResult.ValidationErrors);

        var organisationSubTypeValidationResult = ValidateOrganisationSubType(rows, duplicateValidationResult.TotalErrors);
        validationErrors.AddRange(organisationSubTypeValidationResult.ValidationErrors);

        _logger.LogInformation("Total validation errors {Count}", organisationSubTypeValidationResult.TotalErrors);
        return validationErrors;
    }

    public async Task<(int TotalErrors, List<RegistrationValidationError> ValidationErrors)> ValidateRowsAsync(IList<OrganisationDataRow> rows)
    {
        List<RegistrationValidationError> validationErrors = new();
        int totalErrors = 0;
        foreach (var row in rows.TakeWhile(_ => totalErrors < _validationSettings.ErrorLimit))
        {
            var result = await _organisationDataRowValidator.ValidateAsync(row);

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

    public async Task<List<string>> ValidateAppendedFileAsync<T>(List<T> rows)
        where T : ICsvDataRow
    {
        List<string> errors = new();
        foreach (var row in rows.TakeWhile(_ => errors.Count < _validationSettings.ErrorLimit))
        {
            var result = await ValidateRowAsync(row);

            if (result.IsValid)
            {
                _logger.LogInformation("Row {Row} validated successfully", row.LineNumber);
                continue;
            }

            foreach (var validationError in result.Errors)
            {
                if (!errors.Contains(validationError.ErrorCode))
                {
                    errors.Add(validationError.ErrorCode);
                }

                LogValidationWarning(row.LineNumber, validationError);
            }
        }

        return errors;
    }

    public (int TotalErrors, List<RegistrationValidationError> ValidationErrors) ValidateOrganisationSubType(IList<OrganisationDataRow> rows, int totalErrors)
    {
        List<RegistrationValidationError> validationErrors = new();

        var headOrgTypeCodes = new List<string>()
        {
            OrganisationSubTypeCodes.Licensor,
            OrganisationSubTypeCodes.PubOperator,
            OrganisationSubTypeCodes.Franchisor,
        };

        var subOrgTypeCodes = new List<string>()
        {
            OrganisationSubTypeCodes.Franchisee,
            OrganisationSubTypeCodes.Tenant,
            OrganisationSubTypeCodes.Others,
        };

        var orgSubTypeCode =
            _metaDataProvider.GetOrganisationColumnMetaData(nameof(OrganisationDataRow.OrganisationSubTypeCode));

        var headOrgs = rows
                .Where(row => headOrgTypeCodes.Contains(row.OrganisationSubTypeCode));

        var childOrgsIds = rows
                .Where(row => subOrgTypeCodes.Contains(row.OrganisationSubTypeCode) && !string.IsNullOrEmpty(row.SubsidiaryId))
                .Select(x => x.DefraId).ToHashSet();

        foreach (var invalidHeadOrg in headOrgs
            .Where(x => !childOrgsIds.Contains(x.DefraId))
            .TakeWhile(_ => totalErrors < _validationSettings.ErrorLimit))
        {
            var orgSubTypeCodeValidationError = new ColumnValidationError
            {
                ErrorCode = ErrorCodes.HeadOrganisationMissingSubOrganisation,
                ColumnIndex = orgSubTypeCode?.Index,
                ColumnName = orgSubTypeCode?.Name,
            };

            var error = new RegistrationValidationError
            {
                RowNumber = invalidHeadOrg.LineNumber,
                OrganisationId = invalidHeadOrg.DefraId,
                SubsidiaryId = invalidHeadOrg.SubsidiaryId,
            };
            error.ColumnErrors.Add(orgSubTypeCodeValidationError);

            var errorMessage = $"Head organisation of type {invalidHeadOrg.OrganisationSubTypeCode} must have sub type underneath it";

            LogValidationWarning(invalidHeadOrg.LineNumber, errorMessage, ErrorCodes.HeadOrganisationMissingSubOrganisation);
            validationErrors.Add(error);
            totalErrors++;
        }

        return (totalErrors, validationErrors);
    }

    public bool IsColumnLengthExceeded(List<OrganisationDataRow> rows)
    {
        var columnProperties = typeof(OrganisationDataRow)
            .GetProperties()
            .Where(x => x.GetCustomAttribute<ColumnAttribute>() != null)
            .ToList();

        foreach (var dataRow in rows)
        {
            if (DoesExceedMaxCharacterLength(dataRow, columnProperties))
            {
                return true;
            }
        }

        return false;
    }

    private bool DoesExceedMaxCharacterLength(OrganisationDataRow row, List<PropertyInfo> columnProperties)
    {
        foreach (var property in columnProperties)
        {
            var propertyValue = property.GetValue(row);
            if (propertyValue != null && (propertyValue.ToString().Length > CharacterLimits.MaxLength))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<ValidationResult> ValidateRowAsync<T>(T row)
    {
        return row switch
        {
            BrandDataRow dataRow => await _brandDataRowValidator.ValidateAsync(dataRow),
            PartnersDataRow dataRow => await _partnerDataRowValidator.ValidateAsync(dataRow),
            _ => throw new ArgumentException("Unsupported row type"),
        };
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