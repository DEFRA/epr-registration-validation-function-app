namespace EPR.RegistrationValidation.Application.Services;

using Clients;
using Data.Constants;
using Data.Enums;
using Data.Models;
using Data.Models.QueueMessages;
using Data.Models.SubmissionApi;
using Exceptions;
using Helpers;
using Microsoft.Extensions.Logging;
using Providers;
using Readers;
using static Helpers.RegistrationEventBuilder;

public class RegistrationService : IRegistrationService
{
    private readonly IDequeueProvider _dequeueProvider;
    private readonly IBlobReader _blobReader;
    private readonly ICsvStreamParser _csvStreamParser;
    private readonly ISubmissionApiClient _submissionApiClient;
    private readonly ILogger<RegistrationService> _log;

    public RegistrationService(
        IDequeueProvider dequeueProvider,
        IBlobReader blobReader,
        ICsvStreamParser csvStreamParser,
        ISubmissionApiClient submissionApiClient,
        ILogger<RegistrationService> log)
    {
        _dequeueProvider = dequeueProvider;
        _blobReader = blobReader;
        _csvStreamParser = csvStreamParser;
        _submissionApiClient = submissionApiClient;
        _log = log;
    }

    public async Task ProcessServiceBusMessage(string message)
    {
        var blobQueueMessage = _dequeueProvider.GetMessageFromJson<BlobQueueMessage>(message);
        if (blobQueueMessage.SubmissionSubType != SubmissionSubType.CompanyDetails.ToString())
        {
            _log.LogWarning("Submission sub type is not CompanyDetails");
            return;
        }

        RegistrationEvent registrationEvent;
        try
        {
            var blobMemoryStream = _blobReader.DownloadBlobToStream(blobQueueMessage.BlobName);
            var csvItems = _csvStreamParser.GetItemsFromCsvStream<CsvDataRow>(blobMemoryStream);
            registrationEvent = BuildRegistrationEvent(csvItems, null, blobQueueMessage.BlobName);
        }
        catch (CsvParseException)
        {
            var errorList = new List<string> { ErrorCodes.FileFormatInvalid };
            registrationEvent = BuildErrorRegistrationEvent(errorList, blobQueueMessage.BlobName);
            _log.LogError($"Error parsing CSV");
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
        catch (HttpRequestException)
        {
            _log.LogError($"Error sending event registration message");
        }
    }
}