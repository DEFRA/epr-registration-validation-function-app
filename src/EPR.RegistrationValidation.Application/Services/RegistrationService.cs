namespace EPR.RegistrationValidation.Application.Services;

using Clients;
using Data.Config;
using Data.Constants;
using Data.Enums;
using Data.Models;
using Data.Models.QueueMessages;
using Data.Models.SubmissionApi;
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

    public RegistrationService(
        IDequeueProvider dequeueProvider,
        IBlobReader blobReader,
        ICsvStreamParser csvStreamParser,
        ISubmissionApiClient submissionApiClient,
        IOptions<StorageAccountConfig> options,
        IFeatureManager featureManager,
        IValidationService validationService,
        ILogger<RegistrationService> logger)
    {
        _dequeueProvider = dequeueProvider;
        _blobReader = blobReader;
        _csvStreamParser = csvStreamParser;
        _submissionApiClient = submissionApiClient;
        _options = options.Value;
        _featureManager = featureManager;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task ProcessServiceBusMessage(string message)
    {
        var blobQueueMessage = _dequeueProvider.GetMessageFromJson<BlobQueueMessage>(message);

        if (blobQueueMessage.SubmissionSubType != SubmissionSubType.CompanyDetails.ToString())
        {
            _logger.LogWarning("Submission sub type is not CompanyDetails");
            return;
        }

        RegistrationEvent registrationEvent;
        try
        {
            using var blobMemoryStream = _blobReader.DownloadBlobToStream(blobQueueMessage.BlobName);
            var csvItems = await _csvStreamParser.GetItemsFromCsvStreamAsync<OrganisationDataRow>(blobMemoryStream);
            var errors = new List<string>();
            if (!csvItems.Any())
            {
                _logger.LogInformation("The CSV file for submission ID {submissionId} is empty", blobQueueMessage.SubmissionId);
                errors.Add(ErrorCodes.CsvFileEmptyErrorCode);
            }

            var validationErrors = new List<RegistrationValidationError>();
            if (await _featureManager.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            {
                validationErrors = await _validationService.ValidateAsync(csvItems);
            }

            registrationEvent = BuildRegistrationEvent(csvItems, errors, validationErrors, blobQueueMessage.BlobName, _options.BlobContainerName);
        }
        catch (CsvParseException ex)
        {
            var errors = new List<string> { ErrorCodes.FileFormatInvalid };
            registrationEvent = BuildErrorRegistrationEvent(errors, blobQueueMessage.BlobName, _options.BlobContainerName);
            _logger.LogCritical(ex, ex.Message);
        }
        catch (CsvHeaderException ex)
        {
            var errors = new List<string> { ErrorCodes.CsvFileInvalidHeaderErrorCode };
            registrationEvent = BuildErrorRegistrationEvent(errors, blobQueueMessage.BlobName, _options.BlobContainerName);
            _logger.LogCritical(ex, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "An unexpected error has occurred when processing the service bus message");
            throw;
        }

        try
        {
            await _submissionApiClient.SendEventRegistrationMessage(
                blobQueueMessage.OrganisationId,
                blobQueueMessage.UserId,
                blobQueueMessage.SubmissionId,
                blobQueueMessage.UserType,
                registrationEvent);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error sending event registration message");
        }
    }
}