﻿namespace EPR.RegistrationValidation.Application.Services;

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

        ValidationEvent validationEvent = null;
        try
        {
            switch (blobQueueMessage.SubmissionSubType)
            {
                case nameof(SubmissionSubType.CompanyDetails):
                    validationEvent = await ValidateRegistrationFile(blobQueueMessage);
                    break;
                case nameof(SubmissionSubType.Brands):
                    validationEvent = await ValidateFile<BrandDataRow>(blobQueueMessage);
                    break;
                case nameof(SubmissionSubType.Partnerships):
                    validationEvent = await ValidateFile<PartnersDataRow>(blobQueueMessage);
                    break;
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
                validationEvent);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error sending event registration message");
        }
    }

    private async Task<ValidationEvent> ValidateRegistrationFile(BlobQueueMessage blobQueueMessage)
    {
        var csvRows = await ParseFile<OrganisationDataRow>(blobQueueMessage);

        if (!csvRows.Any())
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

        var validationErrors = new List<RegistrationValidationError>();
        if (await _featureManager.IsEnabledAsync(FeatureFlags.EnableRowValidation) &&
            await _featureManager.IsEnabledAsync(FeatureFlags.EnableOrganisationDataRowValidation))
        {
            validationErrors = await _validationService.ValidateOrganisationsAsync(csvRows);
        }

        return CreateValidationEvent(
            csvRows,
            validationErrors,
            blobQueueMessage.BlobName,
            _options.BlobContainerName);
    }

    private async Task<ValidationEvent> ValidateFile<T>(BlobQueueMessage blobQueueMessage)
        where T : ICsvDataRow
    {
        var csvRows = await ParseFile<T>(blobQueueMessage);

        if (!csvRows.Any())
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

        List<string> fileErrors = new();
        if (await _featureManager.IsEnabledAsync(FeatureFlags.EnableRowValidation) &&
            await _featureManager.IsEnabledAsync(FeatureFlags.EnableBrandPartnerDataRowValidation))
        {
            fileErrors = await _validationService.ValidateAppendedFileAsync(csvRows);
        }

        return CreateValidationEvent(
            GetEventType(blobQueueMessage.SubmissionSubType),
            blobQueueMessage.BlobName,
            _options.BlobContainerName,
            fileErrors.ToArray());
    }

    private async Task<List<T>> ParseFile<T>(BlobQueueMessage blobQueueMessage)
    {
        using var blobMemoryStream = _blobReader.DownloadBlobToStream(blobQueueMessage.BlobName);
        return await _csvStreamParser.GetItemsFromCsvStreamAsync<T>(blobMemoryStream);
    }

    private EventType GetEventType(string submissionSubType)
    {
        switch (submissionSubType)
        {
            case nameof(SubmissionSubType.CompanyDetails):
                return EventType.Registration;
            case nameof(SubmissionSubType.Brands):
                return EventType.BrandValidation;
            case nameof(SubmissionSubType.Partnerships):
                return EventType.PartnerValidation;
        }

        throw new Exception("Invalid submissionSubType");
    }
}