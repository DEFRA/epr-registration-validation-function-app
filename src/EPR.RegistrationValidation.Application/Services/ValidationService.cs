namespace EPR.RegistrationValidation.Application.Services;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using EPR.RegistrationValidation.Application.Clients;
using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Helpers;
using EPR.RegistrationValidation.Application.Services.Subsidiary;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Attributes;
using EPR.RegistrationValidation.Data.Config;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.Data.Models.CompanyDetailsApi;
using EPR.RegistrationValidation.Data.Models.OrganisationDataLookup;
using EPR.RegistrationValidation.Data.Models.QueueMessages;
using EPR.RegistrationValidation.Data.Models.Services;
using EPR.RegistrationValidation.Data.Models.SubmissionApi;
using EPR.RegistrationValidation.Data.Models.Subsidiary;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

public class ValidationService : IValidationService
{
    private readonly string[] _codes =
    {
        UnIncorporationTypeCodes.CoOperative,
        UnIncorporationTypeCodes.SoleTrader,
        UnIncorporationTypeCodes.Partnership,
        UnIncorporationTypeCodes.Others,
        UnIncorporationTypeCodes.OutsideUk,
        HybridCorporationType.CommunityInterestCompany,
    };

    private readonly OrganisationDataRowValidator _organisationDataRowValidator;
    private readonly OrganisationDataRowWarningValidator _organisationDataRowWarningValidator;
    private readonly BrandDataRowValidator _brandDataRowValidator;
    private readonly PartnerDataRowValidator _partnerDataRowValidator;
    private readonly ColumnMetaDataProvider _metaDataProvider;
    private readonly ValidationSettings _validationSettings;
    private readonly RegistrationSettings _registrationSettings;
    private readonly ILogger<ValidationService> _logger;
    private readonly IFeatureManager _featureManager;
    private readonly ICompanyDetailsApiClient _companyDetailsApiClient;
    private readonly ISubmissionApiClient _submissionApiClient;
    private readonly ISubsidiaryDetailsRequestBuilder _subsidiaryDetailsRequestBuilder;
    private Dictionary<string, CompanyDetailsDataResult> _companyDetailsLookup;
    private Dictionary<string, CompanyDetailsDataResult> _complianceSchemeMembersLookup;

    public ValidationService(
        RowValidators rowValidators,
        ColumnMetaDataProvider metaDataProvider,
        ValidationConfig config,
        ApiClients apiClients,
        ILogger<ValidationService> logger,
        IFeatureManager featureManager,
        ISubsidiaryDetailsRequestBuilder subsidiaryDetailsRequestBuilder)
    {
        _organisationDataRowValidator = rowValidators.OrganisationDataRowValidator;
        _organisationDataRowWarningValidator = rowValidators.OrganisationDataRowWarningValidator;
        _brandDataRowValidator = rowValidators.BrandDataRowValidator;
        _partnerDataRowValidator = rowValidators.PartnerDataRowValidator;
        _metaDataProvider = metaDataProvider;
        _companyDetailsApiClient = apiClients.CompanyDetailsApiClient;
        _submissionApiClient = apiClients.SubmissionApiClient;
        _logger = logger;
        _featureManager = featureManager;
        _validationSettings = config.ValidationSettings.Value;
        _registrationSettings = config.RegistrationSettings.Value;
        _subsidiaryDetailsRequestBuilder = subsidiaryDetailsRequestBuilder;
    }

    public async Task<List<RegistrationValidationError>> ValidateOrganisationsAsync(List<OrganisationDataRow> rows, BlobQueueMessage blobQueueMessage, bool validateCompanyDetailsData)
    {
        List<RegistrationValidationError> validationErrors = new();

        var organisationFileDetails = await _submissionApiClient.GetOrganisationFileDetails(blobQueueMessage.SubmissionId, blobQueueMessage.BlobName);

        var rowValidationResult = await ValidateRowsAsync(rows, !string.IsNullOrEmpty(blobQueueMessage.ComplianceSchemeId), organisationFileDetails.SubmissionPeriod);
        validationErrors.AddRange(rowValidationResult.ValidationErrors);

        var organisationSubsidiaryRelationshipsResult = ValidateOrganisationSubsidiaryRelationships(rows, rowValidationResult.TotalErrors);
        validationErrors.AddRange(organisationSubsidiaryRelationshipsResult.ValidationErrors);

        var duplicateValidationResult = ValidateDuplicates(rows, rowValidationResult.TotalErrors);
        validationErrors.AddRange(duplicateValidationResult.ValidationErrors);

        if (validateCompanyDetailsData)
        {
            var companyDetailsValidationResult = await ValidateCompanyDetails(new ValidateCompanyDetailsModel
            {
                OrganisationDataRows = rows,
                TotalErrors = duplicateValidationResult.TotalErrors,
                ComplianceSchemeId = blobQueueMessage.ComplianceSchemeId,
                UserId = blobQueueMessage.UserId,
                ProducerOrganisationId = blobQueueMessage.OrganisationId,
            });

            validationErrors.AddRange(companyDetailsValidationResult.ValidationErrors);
            _logger.LogInformation("Total validation errors {Count}", companyDetailsValidationResult.TotalErrors);
        }
        else
        {
            _logger.LogInformation("Total validation errors {Count}", duplicateValidationResult.TotalErrors);
        }

        if (await _featureManager.IsEnabledAsync(FeatureFlags.EnableSubsidiaryValidation))
        {
            var subValidationResult = await ValidateSubsidiary(rows, rowValidationResult.TotalErrors, validationErrors);
            validationErrors.AddRange(subValidationResult.ValidationErrors);
        }

        return validationErrors;
    }

    public async Task<List<RegistrationValidationWarning>> ValidateOrganisationWarningsAsync(List<OrganisationDataRow> rows)
    {
        List<RegistrationValidationWarning> validationWarnings = new();
        int totalWarnings = 0;

        foreach (var row in rows.TakeWhile(_ => totalWarnings < _validationSettings.ErrorLimit))
        {
            var result = await _organisationDataRowWarningValidator.ValidateAsync(row);

            if (result.IsValid)
            {
                continue;
            }

            var warning = new RegistrationValidationWarning
            {
                RowNumber = row.LineNumber,
                OrganisationId = row.DefraId,
                SubsidiaryId = row.SubsidiaryId,
            };

            foreach (var validationWarning in result.Errors.TakeWhile(_ => totalWarnings < _validationSettings.ErrorLimit))
            {
                var columnMeta = _metaDataProvider.GetOrganisationColumnMetaData(validationWarning.PropertyName);
                warning.ColumnErrors.Add(new ColumnValidationError
                {
                    ErrorCode = validationWarning.ErrorCode,
                    ColumnIndex = columnMeta?.Index,
                    ColumnName = columnMeta?.Name,
                });

                LogValidationWarning(row.LineNumber, validationWarning);
                totalWarnings++;
            }

            validationWarnings.Add(warning);
        }

        return validationWarnings;
    }

    public async Task<(int TotalErrors, List<RegistrationValidationError> ValidationErrors)> ValidateRowsAsync(IList<OrganisationDataRow> rows, bool uploadedByComplianceScheme, string submissionPeriod)
    {
        bool isSubmissionPeriod2026 = string.Equals(submissionPeriod, _registrationSettings.SubmissionPeriod2026, StringComparison.OrdinalIgnoreCase);
        _organisationDataRowValidator.RegisterValidators(uploadedByComplianceScheme, isSubmissionPeriod2026, _registrationSettings.SmallProducersRegStartTime2026, _registrationSettings.SmallProducersRegEndTime2026);

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

    public (int TotalErrors, List<RegistrationValidationError> ValidationErrors) ValidateOrganisationSubsidiaryRelationships(List<OrganisationDataRow> rows, int totalErrors)
    {
        List<RegistrationValidationError> validationErrors = new();

        var defraIdColumn = _metaDataProvider.GetOrganisationColumnMetaData(nameof(OrganisationDataRow.DefraId));

        var defraIdValidationError = new ColumnValidationError
        {
            ErrorCode = ErrorCodes.MissingOrganisationId,
            ColumnIndex = defraIdColumn?.Index,
            ColumnName = defraIdColumn?.Name,
        };

        foreach (var row in rows.Where(x => !string.IsNullOrEmpty(x.SubsidiaryId)))
        {
            if (!rows.Exists(x => x.DefraId == row.DefraId && string.IsNullOrEmpty(x.SubsidiaryId)))
            {
                var error = new RegistrationValidationError
                {
                    RowNumber = row.LineNumber,
                    OrganisationId = row.DefraId,
                    SubsidiaryId = row.SubsidiaryId,
                };
                error.ColumnErrors.Add(defraIdValidationError);

                var errorMessage = $"Organisation parent record not found for organisation id = {row.DefraId} subsidiary id = {row.SubsidiaryId}";

                LogValidationWarning(row.LineNumber, errorMessage, ErrorCodes.MissingOrganisationId);
                validationErrors.Add(error);
                totalErrors++;
            }
        }

        return (totalErrors, validationErrors);
    }

    public async Task<List<string>> ValidateAppendedFileAsync<T>(List<T> rows, OrganisationDataLookupTable organisationLookup)
        where T : ICsvDataRow
    {
        var errors = await ValidateRows(rows, organisationLookup);

        var missingOrganisationErrors = await ValidateAllOrganisationRowsInFile(rows, organisationLookup);
        errors.AddRange(missingOrganisationErrors
                        .Where(e => !errors.Contains(e)));

        return errors;
    }

    public async Task<(int TotalErrors, List<RegistrationValidationError> ValidationErrors)> ValidateCompanyDetails(ValidateCompanyDetailsModel validateCompanyDetailsModel)
    {
        List<RegistrationValidationError> validationErrors = new();

        try
        {
            _companyDetailsLookup = new Dictionary<string, CompanyDetailsDataResult>();
            _complianceSchemeMembersLookup = new Dictionary<string, CompanyDetailsDataResult>();

            var unvalidatedComplianceSchemeRows = new List<OrganisationDataRow>();

            foreach (var row in validateCompanyDetailsModel.OrganisationDataRows.TakeWhile(_ => validateCompanyDetailsModel.TotalErrors < _validationSettings.ErrorLimit))
            {
                if (string.IsNullOrEmpty(validateCompanyDetailsModel.ComplianceSchemeId))
                {
                    var producerValidationResult = await ValidateAsProducer(row, validateCompanyDetailsModel.ProducerOrganisationId);
                    validateCompanyDetailsModel.TotalErrors += producerValidationResult.TotalErrors;
                    validationErrors.AddRange(producerValidationResult.ValidationErrors);
                }
                else
                {
                    var complianceSchemeValidationResult = await ValidateAsComplianceSchemeUser(validateCompanyDetailsModel.ComplianceSchemeId, row);
                    validateCompanyDetailsModel.TotalErrors += complianceSchemeValidationResult.TotalErrors;
                    validationErrors.AddRange(complianceSchemeValidationResult.ValidationErrors);

                    if (!complianceSchemeValidationResult.ValidComplianceSchemeMember)
                    {
                        unvalidatedComplianceSchemeRows.Add(row);
                    }
                }
            }

            if (!string.IsNullOrEmpty(validateCompanyDetailsModel.ComplianceSchemeId) && unvalidatedComplianceSchemeRows.Count > 0)
            {
                var remainingMembersValidationResult = await ValidateRemainingComplianceSchemeMembers(unvalidatedComplianceSchemeRows);

                validateCompanyDetailsModel.TotalErrors += remainingMembersValidationResult.TotalErrors;
                validationErrors.AddRange(remainingMembersValidationResult.ValidationErrors);
            }

            return (validateCompanyDetailsModel.TotalErrors, validationErrors);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error comparing organisation details");
            return (validateCompanyDetailsModel.TotalErrors, validationErrors);
        }
    }

    public async Task<(int TotalErrors, List<RegistrationValidationError> ValidationErrors)> ValidateSubsidiary(List<OrganisationDataRow> rows, int totalErrors, List<RegistrationValidationError> existingErrors)
    {
        List<RegistrationValidationError> validationErrors = new();

        try
        {
            var subsidiaryDetailsRequest = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);
            if (subsidiaryDetailsRequest?.SubsidiaryOrganisationDetails == null || !subsidiaryDetailsRequest.SubsidiaryOrganisationDetails.Any())
            {
                return (totalErrors, validationErrors);
            }

            var result = await _companyDetailsApiClient.GetSubsidiaryDetails(subsidiaryDetailsRequest);
            var errorLimit = _validationSettings.ErrorLimit;

            foreach (var row in rows.TakeWhile(_ => totalErrors < errorLimit))
            {
                var matchingSub = FindMatchingSubsidiary(result, row.DefraId, row.SubsidiaryId);
                if (matchingSub == null)
                {
                    continue;
                }

                if (!matchingSub.SubsidiaryExists)
                {
                    totalErrors = AddValidationError(row, ErrorCodes.SubsidiaryIdDoesNotExist, "Subsidiary ID does not exist", validationErrors, totalErrors, errorLimit);
                    continue;
                }

                if (matchingSub.SubsidiaryBelongsToAnyOtherOrganisation)
                {
                    totalErrors = AddValidationError(row, ErrorCodes.SubsidiaryIdBelongsToDifferentOrganisation, "Subsidiary ID is assigned to a different organisation", validationErrors, totalErrors, errorLimit);
                }

                if (matchingSub.SubsidiaryDoesNotBelongToAnyOrganisation)
                {
                    totalErrors = AddValidationError(row, ErrorCodes.SubsidiaryDoesNotBelongToAnyOrganisation, "Subsidiary ID does not belong to any organisation", validationErrors, totalErrors, errorLimit);
                }

                if (!string.Equals(row.CompaniesHouseNumber, matchingSub.CompaniesHouseNumber))
                {
                    totalErrors = AddValidationError(row, ErrorCodes.SubsidiaryIdDoesNotMatchCompaniesHouseNumber, "This companies house number does not match the subsidiary ID", validationErrors, totalErrors, errorLimit, nameof(OrganisationDataRow.CompaniesHouseNumber));
                }
            }

            return (totalErrors, validationErrors);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error during Subsidiary validation");
            return (totalErrors, validationErrors);
        }
    }

    public bool IsColumnLengthExceeded(List<OrganisationDataRow> rows)
    {
        var columnProperties = typeof(OrganisationDataRow)
            .GetProperties()
            .Where(x => x.GetCustomAttribute<ColumnAttribute>() != null)
            .ToList();

        return rows.Exists(x => DoesExceedMaxCharacterLength(x, columnProperties));
    }

    private static List<OrganisationIdentifiers?> GetMissingOrganisationRows(
    Dictionary<string, Dictionary<string, OrganisationIdentifiers>> organisationData,
    IEnumerable<OrganisationIdentifiers> rowIdentifiers)
    {
        return organisationData
            .SelectMany(x => x.Value.Values, (_, b) => new { b.DefraId, b.SubsidiaryId })
            .Where(x => !rowIdentifiers.Any(row => row.DefraId == x.DefraId))
            .Select(x => new OrganisationIdentifiers(x.DefraId, x.SubsidiaryId))
            .ToList();
    }

    private static List<OrganisationIdentifiers?> GetMissingSubsidiaryRows(
    Dictionary<string, Dictionary<string, OrganisationIdentifiers>> organisationData,
    IEnumerable<OrganisationIdentifiers> rowIdentifiers)
    {
        return organisationData
            .SelectMany(x => x.Value.Values, (_, b) => new { b.DefraId, b.SubsidiaryId })
            .Where(x => rowIdentifiers.Any(row => row.DefraId == x.DefraId)
                && !string.IsNullOrEmpty(x.SubsidiaryId)
                && !rowIdentifiers.Any(row => row.SubsidiaryId == x.SubsidiaryId))
            .Select(x => new OrganisationIdentifiers(x.DefraId, x.SubsidiaryId))
            .ToList();
    }

    private static string GetMissingOrganisationErrorCode<T>(List<T> rows, IList<OrganisationIdentifiers> missingIdentifiers)
    {
        if (missingIdentifiers.Any())
        {
            return rows switch
            {
                List<BrandDataRow> _ => ErrorCodes.BrandDetailsNotMatchingOrganisation,
                List<PartnersDataRow> _ => ErrorCodes.PartnerDetailsNotMatchingOrganisation,
            };
        }

        return null;
    }

    private static string GetMissingSubsidiaryErrorCode<T>(List<T> rows, IEnumerable<OrganisationIdentifiers> missingIdentifiers)
    {
        if (missingIdentifiers.Any())
        {
            return rows switch
            {
                List<BrandDataRow> _ => ErrorCodes.BrandDetailsNotMatchingSubsidiary,
                List<PartnersDataRow> _ => ErrorCodes.PartnerDetailsNotMatchingSubsidiary,
            };
        }

        return null;
    }

    private static bool DoesExceedMaxCharacterLength(OrganisationDataRow row, List<PropertyInfo> columnProperties)
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

    private static async Task<bool> IsValidComplianceSchemeMember(OrganisationDataRow row, CompanyDetailsDataResult companyDetails)
    {
        if (companyDetails?.Organisations == null ||
            !companyDetails.Organisations.Any(x => x.ReferenceNumber == row.DefraId))
        {
            return false;
        }

        return true;
    }

    private static SubsidiaryDetail FindMatchingSubsidiary(SubsidiaryDetailsResponse result, string defraId, string subsidiaryId)
    {
        return result.SubsidiaryOrganisationDetails
            ?.Find(org => org.OrganisationReference == defraId)
            ?.SubsidiaryDetails
            ?.Find(sub => sub.ReferenceNumber == subsidiaryId);
    }

    private static ValidationContext<T> CreateValidationContextWithLookupData<T>(T row, OrganisationDataLookupTable organisationDataLookup)
    {
        var context = new ValidationContext<T>(row);
        if (organisationDataLookup?.Data is not null && organisationDataLookup.Data.Count > 0)
        {
            context.RootContextData[nameof(OrganisationDataLookupTable)] = organisationDataLookup;
        }

        return context;
    }

    private int AddValidationError(
        OrganisationDataRow row,
        string errorCode,
        string errorMessage,
        List<RegistrationValidationError> validationErrors,
        int totalErrors,
        int errorLimit,
        string columnName = null)
    {
        if (totalErrors >= errorLimit)
        {
            return totalErrors;
        }

        var error = columnName == null
            ? CreateSubValidationError(row, errorCode)
            : CreateColumnValidationError(row, errorCode, columnName);

        LogValidationWarning(row.LineNumber, errorMessage, errorCode);
        validationErrors.Add(error);

        return totalErrors + 1;
    }

    private async Task<List<string>> ValidateRows<T>(List<T> rows, OrganisationDataLookupTable organisationDataLookup)
        where T : ICsvDataRow
    {
        List<string> errors = new();
        foreach (var row in rows.TakeWhile(_ => errors.Count < _validationSettings.ErrorLimit))
        {
            var result = await ValidateRowAsync(row, organisationDataLookup);

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

    private async Task<ValidationResult> ValidateRowAsync<T>(T row, OrganisationDataLookupTable organisationLookup)
    {
        return row switch
        {
            BrandDataRow dataRow => await _brandDataRowValidator.ValidateAsync(CreateValidationContextWithLookupData(dataRow, organisationLookup)),
            PartnersDataRow dataRow => await _partnerDataRowValidator.ValidateAsync(CreateValidationContextWithLookupData(dataRow, organisationLookup)),
            _ => throw new ArgumentException("Unsupported row type"),
        };
    }

    private async Task<List<string>> ValidateAllOrganisationRowsInFile<T>(List<T> rows, OrganisationDataLookupTable organisationDataLookup)
    {
        List<string> errors = new();

        if (organisationDataLookup?.Data is null)
        {
            return errors;
        }

        var rowIdentifiers = rows switch
        {
            List<BrandDataRow> _ => rows.Cast<BrandDataRow>().Select(x => new OrganisationIdentifiers(x.DefraId, x.SubsidiaryId)),
            List<PartnersDataRow> _ => rows.Cast<PartnersDataRow>().Select(x => new OrganisationIdentifiers(x.DefraId, x.SubsidiaryId)),
            _ => throw new ArgumentException("Unsupported row type"),
        };

        var missingOrganisationRows = GetMissingOrganisationRows(organisationDataLookup.Data, rowIdentifiers);
        var missingOrganisationErrorCode = GetMissingOrganisationErrorCode(rows, missingOrganisationRows);

        var missingSubsidiaryRows = GetMissingSubsidiaryRows(organisationDataLookup.Data, rowIdentifiers);
        var missingSubsidiaryErrorCode = GetMissingSubsidiaryErrorCode(rows, missingSubsidiaryRows);

        if (!string.IsNullOrEmpty(missingOrganisationErrorCode))
        {
            errors.Add(missingOrganisationErrorCode);
            LogMissingIdentifierErrors(missingOrganisationRows, missingOrganisationErrorCode);
        }

        if (!string.IsNullOrEmpty(missingSubsidiaryErrorCode) && !errors.Contains(missingSubsidiaryErrorCode))
        {
            errors.Add(missingSubsidiaryErrorCode);
            LogMissingIdentifierErrors(missingSubsidiaryRows, missingSubsidiaryErrorCode);
        }

        return errors;
    }

    private void LogMissingIdentifierErrors(IEnumerable<OrganisationIdentifiers> missingIdentifiers, string errorCode)
    {
        if (!missingIdentifiers.Any())
        {
            return;
        }

        foreach (var missingRow in missingIdentifiers)
        {
            _logger.LogWarning(
                "Validation error - no row in organisation registration file with organisation id {DefraId} and subsidiary id {SubsidiaryId}. Error code {ErrorCode}",
                missingRow.DefraId,
                missingRow.SubsidiaryId,
                errorCode);
        }
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

    private void LogValidationWarning(int row, int column, string errorMessage, string errorCode)
    {
        _logger.LogWarning(
            "Validation error for column {Column} on row {Row} {ErrorMessage} Error code {ErrorCode}",
            column,
            row,
            errorMessage,
            errorCode);
    }

    private async Task<(int TotalErrors, List<RegistrationValidationError> ValidationErrors)> ValidateCompaniesHouseNumbers(OrganisationDataRow row, CompanyDetailsDataResult companyDetails)
    {
        List<RegistrationValidationError> validationErrors = new();
        int totalErrors = 0;
        var organisationId = _metaDataProvider.GetOrganisationColumnMetaData(nameof(OrganisationDataRow.DefraId));
        var companiesHouseNumber = _metaDataProvider.GetOrganisationColumnMetaData(nameof(OrganisationDataRow.CompaniesHouseNumber));

        var organisation = companyDetails?.Organisations?.FirstOrDefault(x => x.ReferenceNumber == row.DefraId);

        if (companyDetails != null && organisation != null && string.IsNullOrEmpty(row.SubsidiaryId))
        {
            if (string.IsNullOrEmpty(row.CompaniesHouseNumber) && string.IsNullOrEmpty(organisation.CompaniesHouseNumber))
            {
                return (totalErrors, validationErrors);
            }

            if (Array.Exists(_codes, typeCode => string.Equals(typeCode, row.OrganisationTypeCode, StringComparison.OrdinalIgnoreCase)))
            {
                return (totalErrors, validationErrors);
            }

            if (row.CompaniesHouseNumber != organisation.CompaniesHouseNumber)
            {
                var organisationIdColumnValidationError = new ColumnValidationError
                {
                    ErrorCode = ErrorCodes.CompaniesHouseNumberNotMatchOrganisationId,
                    ColumnIndex = organisationId?.Index,
                    ColumnName = organisationId?.Name,
                };

                var companiesHouseNumberColumnValidationError = new ColumnValidationError
                {
                    ErrorCode = ErrorCodes.CompaniesHouseNumberNotMatchOrganisationId,
                    ColumnIndex = companiesHouseNumber?.Index,
                    ColumnName = companiesHouseNumber?.Name,
                };

                var error = new RegistrationValidationError
                {
                    RowNumber = row.LineNumber,
                    OrganisationId = row.DefraId,
                    SubsidiaryId = row.SubsidiaryId,
                };

                error.ColumnErrors.Add(organisationIdColumnValidationError);
                error.ColumnErrors.Add(companiesHouseNumberColumnValidationError);
                var errorMessage = $"Companies House number does not match this organisation ID - check both";
                LogValidationWarning(row.LineNumber, organisationId is null ? 0 : (int)organisationId.Index, errorMessage, ErrorCodes.CompaniesHouseNumberNotMatchOrganisationId);
                LogValidationWarning(row.LineNumber, companiesHouseNumber is null ? 0 : (int)companiesHouseNumber.Index, errorMessage, ErrorCodes.CompaniesHouseNumberNotMatchOrganisationId);
                validationErrors.Add(error);
                totalErrors++;
            }
        }

        return (totalErrors, validationErrors);
    }

    private async Task<(int TotalErrors, List<RegistrationValidationError> ValidationErrors)> ValidateAsProducer(OrganisationDataRow row, string producerOrganistionId)
    {
        List<RegistrationValidationError> validationErrors = new();
        int totalErrors = 0;

        if (!_companyDetailsLookup.TryGetValue(row.DefraId, out var companyDetails))
        {
            companyDetails = await _companyDetailsApiClient.GetCompanyDetailsByProducer(producerOrganistionId);
            _companyDetailsLookup[row.DefraId] = companyDetails;
        }

        if (companyDetails?.Organisations?.FirstOrDefault(x => x.ReferenceNumber == row.DefraId) == null)
        {
            var error = CreateOrganisationIdValidationError(row);

            var errorMessage = $"Check organisation ID - this one is either invalid or for an organisation that does not need to submit data";

            LogValidationWarning(row.LineNumber, errorMessage, ErrorCodes.CheckOrganisationId);

            validationErrors.Add(error);
            totalErrors++;
        }

        var companiesHouseNumberValidationResult = await ValidateCompaniesHouseNumbers(row, companyDetails: companyDetails);
        totalErrors += companiesHouseNumberValidationResult.TotalErrors;
        validationErrors.AddRange(companiesHouseNumberValidationResult.ValidationErrors);

        return (totalErrors, validationErrors);
    }

    private async Task<(int TotalErrors, List<RegistrationValidationError> ValidationErrors)> ValidateRemainingComplianceSchemeMembers(IList<OrganisationDataRow> rows)
    {
        List<RegistrationValidationError> validationErrors = new();
        int totalErrors = 0;
        var organisationIds = rows.Select(r => r.DefraId);
        var remainingProducerDetails = await _companyDetailsApiClient.GetRemainingProducerDetails(organisationIds);
        var producers = new Dictionary<string, string>();

        if (remainingProducerDetails?.Organisations != null)
        {
            producers = remainingProducerDetails.Organisations.ToDictionary(x => x.ReferenceNumber, x => x.CompaniesHouseNumber);
        }

        foreach (var row in rows)
        {
            if (remainingProducerDetails?.Organisations == null ||
                !producers.ContainsKey(row.DefraId))
            {
                var error = CreateOrganisationIdValidationError(row);
                var errorMessage = $"Check organisation ID - this one is either invalid or for an organisation that does not need to submit data";
                LogValidationWarning(row.LineNumber, errorMessage, ErrorCodes.CheckOrganisationId);

                validationErrors.Add(error);
                totalErrors++;
            }

            var companiesHouseNumberValidationResult = await ValidateCompaniesHouseNumbers(row, companyDetails: remainingProducerDetails);
            totalErrors += companiesHouseNumberValidationResult.TotalErrors;
            validationErrors.AddRange(companiesHouseNumberValidationResult.ValidationErrors);
        }

        return (totalErrors, validationErrors);
    }

    private RegistrationValidationError CreateOrganisationIdValidationError(OrganisationDataRow row)
    {
        var organisationIdColumn = _metaDataProvider.GetOrganisationColumnMetaData(nameof(OrganisationDataRow.DefraId));

        var columnValidationError = new ColumnValidationError
        {
            ErrorCode = ErrorCodes.CheckOrganisationId,
            ColumnIndex = organisationIdColumn?.Index,
            ColumnName = organisationIdColumn?.Name,
        };

        var error = new RegistrationValidationError
        {
            RowNumber = row.LineNumber,
            OrganisationId = row.DefraId,
            SubsidiaryId = string.Empty,
        };

        error.ColumnErrors.Add(columnValidationError);

        return error;
    }

    private RegistrationValidationError CreateSubValidationError(OrganisationDataRow row, string errorCode)
    {
        var subColumn = _metaDataProvider.GetOrganisationColumnMetaData(nameof(OrganisationDataRow.SubsidiaryId));

        var columnValidationError = new ColumnValidationError
        {
            ErrorCode = errorCode,
            ColumnIndex = subColumn?.Index,
            ColumnName = subColumn?.Name,
        };

        var error = new RegistrationValidationError
        {
            RowNumber = row.LineNumber,
            OrganisationId = row.DefraId,
            SubsidiaryId = row.SubsidiaryId,
        };

        error.ColumnErrors.Add(columnValidationError);
        return error;
    }

    private RegistrationValidationError CreateColumnValidationError(OrganisationDataRow row, string errorCode, string columnHeader)
    {
        var column = _metaDataProvider.GetOrganisationColumnMetaData(columnHeader);

        var columnValidationError = new ColumnValidationError
        {
            ErrorCode = errorCode,
            ColumnIndex = column?.Index,
            ColumnName = column?.Name,
        };

        var error = new RegistrationValidationError
        {
            RowNumber = row.LineNumber,
            OrganisationId = row.DefraId,
            SubsidiaryId = row.SubsidiaryId,
        };

        error.ColumnErrors.Add(columnValidationError);
        return error;
    }

    private async Task<(int TotalErrors, List<RegistrationValidationError> ValidationErrors, bool ValidComplianceSchemeMember)> ValidateAsComplianceSchemeUser(string complianceSchemeId, OrganisationDataRow row)
    {
        List<RegistrationValidationError> validationErrors = new();
        int totalErrors = 0;
        var complianceSchemeKey = $"{row.DefraId}_{complianceSchemeId}";

        if (!_complianceSchemeMembersLookup.TryGetValue(complianceSchemeKey, out var complianceSchemeMembers))
        {
            complianceSchemeMembers = await _companyDetailsApiClient.GetComplianceSchemeMembers(row.DefraId, complianceSchemeId);
            _complianceSchemeMembersLookup[complianceSchemeKey] = complianceSchemeMembers;
        }

        var validComplianceSchemeMember = await IsValidComplianceSchemeMember(row, complianceSchemeMembers);

        if (validComplianceSchemeMember)
        {
            var companiesHouseNumberValidationResult = await ValidateCompaniesHouseNumbers(row, companyDetails: complianceSchemeMembers);
            totalErrors += companiesHouseNumberValidationResult.TotalErrors;
            validationErrors.AddRange(companiesHouseNumberValidationResult.ValidationErrors);
        }

        return (totalErrors, validationErrors, validComplianceSchemeMember);
    }
}
