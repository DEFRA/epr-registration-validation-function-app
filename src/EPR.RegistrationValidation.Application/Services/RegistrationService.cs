namespace EPR.RegistrationValidation.Application.Services;

using Clients;
using Data.Config;
using Data.Constants;
using Data.Enums;
using Data.Models;
using Data.Models.QueueMessages;
using Data.Models.SubmissionApi;
using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Data.Models.OrganisationDataLookup;
using Exceptions;
using Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Providers;
using Readers;
using static Helpers.RegistrationEventBuilder;

public class RegistrationService : IRegistrationService
{
    private readonly IDequeueProvider _dequeueProvider;
    private readonly IBlobReader _blobReader;
    private readonly ICsvStreamParser _csvStreamParser;
    private readonly ISubmissionApiClient _submissionApiClient;
    private readonly StorageAccountConfig _options;
    private readonly ILogger<RegistrationService> _logger;
    private readonly IFeatureManager _featureManager;
    private readonly IValidationService _validationService;
    private readonly ValidationSettings _validationSettings;

    public RegistrationService(
        IDequeueProvider dequeueProvider,
        IBlobReader blobReader,
        ICsvStreamParser csvStreamParser,
        ISubmissionApiClient submissionApiClient,
        IOptions<StorageAccountConfig> options,
        IFeatureManager featureManager,
        IValidationService validationService,
        ILogger<RegistrationService> logger,
        IOptions<ValidationSettings> validationSettings)
    {
        _dequeueProvider = dequeueProvider;
        _blobReader = blobReader;
        _csvStreamParser = csvStreamParser;
        _submissionApiClient = submissionApiClient;
        _options = options.Value;
        _featureManager = featureManager;
        _validationService = validationService;
        _logger = logger;
        _validationSettings = validationSettings.Value;
    }

    public async Task ProcessServiceBusMessage(string message)
    {
        var blobQueueMessage = _dequeueProvider.GetMessageFromJson<BlobQueueMessage>(message);

        ValidationEvent validationEvent = null;
        try
        {
            switch (blobQueueMessage.SubmissionSubType)
            {
                case nameof(SubmissionSubType.CompanyDetails):
                    validationEvent = await ValidateRegistrationFile(blobQueueMessage);
                    break;
                case nameof(SubmissionSubType.Brands) when blobQueueMessage.RequiresRowValidation == true:
                    validationEvent = await ValidateFile<BrandDataRow>(blobQueueMessage);
                    break;
                case nameof(SubmissionSubType.Partnerships) when blobQueueMessage.RequiresRowValidation == true:
                    validationEvent = await ValidateFile<PartnersDataRow>(blobQueueMessage);
                    break;
                default:
                    _logger.LogWarning("Submission sub type {Type} is not supported or validation for this type is disabled", blobQueueMessage.SubmissionSubType);
                    return;
            }
        }
        catch (CsvParseException ex)
        {
            _logger.LogCritical(ex, ex.Message);

            validationEvent = CreateValidationEvent(
                GetEventType(blobQueueMessage.SubmissionSubType),
                blobQueueMessage.BlobName,
                _options.BlobContainerName,
                ErrorCodes.FileFormatInvalid);
        }
        catch (CsvHeaderException ex)
        {
            _logger.LogCritical(ex, ex.Message);

            validationEvent = CreateValidationEvent(
                GetEventType(blobQueueMessage.SubmissionSubType),
                blobQueueMessage.BlobName,
                _options.BlobContainerName,
                ErrorCodes.CsvFileInvalidHeaderErrorCode);
        }
        catch (CompanyDetailsApiClientException ex)
        {
            _logger.LogCritical(ex, ex.Message);

            validationEvent = CreateValidationEvent(
                GetEventType(blobQueueMessage.SubmissionSubType),
                blobQueueMessage.BlobName,
                _options.BlobContainerName,
                ErrorCodes.UncaughtExceptionErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "An unexpected error has occurred when processing the service bus message");

            validationEvent = CreateValidationEvent(
                GetEventType(blobQueueMessage.SubmissionSubType),
                blobQueueMessage.BlobName,
                _options.BlobContainerName,
                ErrorCodes.UncaughtExceptionErrorCode);
        }

        try
        {
            await _submissionApiClient.SendEventRegistrationMessage(
                blobQueueMessage.OrganisationId,
                blobQueueMessage.UserId,
                blobQueueMessage.SubmissionId,
                blobQueueMessage.UserType,
                validationEvent);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error sending event registration message");
        }
    }

    private static EventType GetEventType(string submissionSubType) =>
        submissionSubType switch
        {
            nameof(SubmissionSubType.CompanyDetails) => EventType.Registration,
            nameof(SubmissionSubType.Brands) => EventType.BrandValidation,
            nameof(SubmissionSubType.Partnerships) => EventType.PartnerValidation,
            _ => throw new ArgumentException($"Invalid submissionSubType: '{submissionSubType}'"),
        };

    private async Task<ValidationEvent> ValidateRegistrationFile(BlobQueueMessage blobQueueMessage)
    {
        var isRowValidationEnabled = await IsRowValidationEnabledAsync();
        var csvRows = await ParseFile<OrganisationDataRow>(
            blobQueueMessage.BlobName,
            !isRowValidationEnabled);

        if (csvRows.Count == 0)
        {
            _logger.LogInformation(
                "The CSV file for submission ID {SubmissionId} is empty",
                blobQueueMessage.SubmissionId);

            return CreateValidationEvent(
                EventType.Registration,
                blobQueueMessage.BlobName,
                _options.BlobContainerName,
                ErrorCodes.CsvFileEmptyErrorCode);
        }

        int? organisationMemberCount = null;

        var validationErrors = new List<RegistrationValidationError>();
        var validationWarnings = new List<RegistrationValidationWarning>();

        if (await IsOrgDataValidationEnabledAsync(blobQueueMessage))
        {
            if (_validationService.IsColumnLengthExceeded(csvRows))
            {
                return CreateValidationEvent(
                    EventType.Registration,
                    blobQueueMessage.BlobName,
                    _options.BlobContainerName,
                    ErrorCodes.CharacterLengthExceeded);
            }

            validationErrors = await _validationService.ValidateOrganisationsAsync(
                csvRows,
                blobQueueMessage,
                await IsCompanyDetailsDataValidationEnabledAsync(blobQueueMessage));

            validationWarnings = await _validationService.ValidateOrganisationWarningsAsync(csvRows);
        }

        if (validationErrors.Count == 0)
        {
            organisationMemberCount = csvRows.GroupBy(row => row?.DefraId).Count();
        }

        return CreateValidationEvent(
            csvRows,
            validationErrors,
            validationWarnings,
            blobQueueMessage.BlobName,
            _options.BlobContainerName,
            _validationSettings.ErrorLimit,
            organisationMemberCount);
    }

    private async Task<ValidationEvent> ValidateFile<T>(BlobQueueMessage blobQueueMessage)
        where T : ICsvDataRow
    {
        List<string> fileErrors = new();

        if (await IsBrandPartnerValidationEnabledAsync())
        {
            var csvRows = await ParseFile<T>(blobQueueMessage.BlobName);
            if (csvRows.Count == 0)
            {
                _logger.LogInformation(
                    "The CSV file for submission ID {SubmissionId} is empty",
                    blobQueueMessage.SubmissionId);

                return CreateValidationEvent(
                    GetEventType(blobQueueMessage.SubmissionSubType),
                    blobQueueMessage.BlobName,
                    _options.BlobContainerName,
                    ErrorCodes.CsvFileEmptyErrorCode);
            }

            var organisationDataLookup = await IsBrandPartnerCrossFileValidationEnabledAsync(blobQueueMessage)
                ? await GetOrganisationFileDetailsLookup<T>(
                    blobQueueMessage.SubmissionId,
                    blobQueueMessage.BlobName)
                : null;

            fileErrors = await _validationService.ValidateAppendedFileAsync(csvRows, organisationDataLookup);
        }

        return CreateValidationEvent(
            GetEventType(blobQueueMessage.SubmissionSubType),
            blobQueueMessage.BlobName,
            _options.BlobContainerName,
            fileErrors.ToArray());
    }

    private async Task<OrganisationDataLookupTable> GetOrganisationFileDetailsLookup<T>(
        string submissionId,
        string blobName)
        where T : ICsvDataRow
    {
        var organisationFileDetails = await _submissionApiClient.GetOrganisationFileDetails(submissionId, blobName);

        if (organisationFileDetails == null || string.IsNullOrEmpty(organisationFileDetails.BlobName))
        {
            _logger.LogInformation(
                "Registration blob for submission ID {SubmissionId} was not found",
                submissionId);

            throw new OrganisationDetailsException($"Registration blob for submission ID {submissionId} was not found");
        }

        _logger.LogInformation("Cross-file check found organisation blob {BlobName}", organisationFileDetails.BlobName);

        var organisationRows = await ParseFile<OrganisationDataRow>(organisationFileDetails.BlobName, true);

        _logger.LogInformation("Cross-file check loaded {OrganisationRows} from ", organisationRows.Count);

        var brandPackagingActivities = new string[]
        {
                    PackagingActivities.Primary,
                    PackagingActivities.Secondary,
        };

        var filteredOrganisationLookup = typeof(T).Name switch
        {
            nameof(BrandDataRow) =>
                organisationRows
                .Where(row => brandPackagingActivities.Contains(row.PackagingActivitySO, StringComparer.OrdinalIgnoreCase))
                .GroupBy(o => o.DefraId)
                .ToDictionary(
                    g => g.Key,
                    g => g.DistinctBy(d => d.SubsidiaryId)
                          .ToDictionary(
                              i => i.SubsidiaryId ?? string.Empty,
                              i => new OrganisationIdentifiers(i.DefraId, i.SubsidiaryId))),

            nameof(PartnersDataRow) =>
                organisationRows
                .Where(row => string.Compare(row.OrganisationTypeCode, UnIncorporationTypeCodes.Partnership, StringComparison.OrdinalIgnoreCase) == 0)
                .GroupBy(o => o.DefraId)
                .ToDictionary(
                    g => g.Key,
                    g => g.DistinctBy(d => d.SubsidiaryId)
                          .ToDictionary(
                              i => i.SubsidiaryId,
                              i => new OrganisationIdentifiers(i.DefraId, i.SubsidiaryId))),
        };

        _logger.LogInformation("Cross-file check found {RowCount} rows for type {TypeName}", filteredOrganisationLookup.Count, typeof(T).Name);

        return new OrganisationDataLookupTable(filteredOrganisationLookup);
    }

    private async Task<List<T>> ParseFile<T>(string blobName, bool useMinimalClassMaps = false)
    {
        using var blobMemoryStream = _blobReader.DownloadBlobToStream(blobName);
        return await _csvStreamParser.GetItemsFromCsvStreamAsync<T>(
            blobMemoryStream,
            useMinimalClassMaps);
    }

    private async Task<bool> IsRowValidationEnabledAsync()
    {
        return await _featureManager.IsEnabledAsync(FeatureFlags.EnableRowValidation);
    }

    private async Task<bool> IsOrgDataValidationEnabledAsync(BlobQueueMessage blobQueueMessage)
    {
        return blobQueueMessage.RequiresRowValidation == true &&
            await _featureManager.IsEnabledAsync(FeatureFlags.EnableRowValidation) &&
            await _featureManager.IsEnabledAsync(FeatureFlags.EnableOrganisationDataRowValidation);
    }

    private async Task<bool> IsBrandPartnerValidationEnabledAsync()
    {
        return await _featureManager.IsEnabledAsync(FeatureFlags.EnableRowValidation) &&
            await _featureManager.IsEnabledAsync(FeatureFlags.EnableBrandPartnerDataRowValidation);
    }

    private async Task<bool> IsBrandPartnerCrossFileValidationEnabledAsync(BlobQueueMessage blobQueueMessage)
    {
        return blobQueueMessage.RequiresRowValidation == true &&
            await _featureManager.IsEnabledAsync(FeatureFlags.EnableRowValidation) &&
            await _featureManager.IsEnabledAsync(FeatureFlags.EnableBrandPartnerCrossFileValidation);
    }

    private async Task<bool> IsCompanyDetailsDataValidationEnabledAsync(BlobQueueMessage blobQueueMessage)
    {
        return blobQueueMessage.RequiresRowValidation == true &&
            await _featureManager.IsEnabledAsync(FeatureFlags.EnableRowValidation) &&
            await _featureManager.IsEnabledAsync(FeatureFlags.EnableOrganisationDataRowValidation) &&
            await _featureManager.IsEnabledAsync(FeatureFlags.EnableCompanyDetailsValidation);
    }
}