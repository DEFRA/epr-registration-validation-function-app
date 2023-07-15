namespace EPR.RegistrationValidation.UnitTests.Services;

using Application.Clients;
using Application.Exceptions;
using Application.Helpers;
using Application.Providers;
using Application.Readers;
using Application.Services;
using Azure.Storage.Blobs;
using Data.Config;
using Data.Enums;
using Data.Models;
using Data.Models.QueueMessages;
using Data.Models.SubmissionApi;
using FluentAssertions;
using Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using TestHelpers;

[TestClass]
public class RegistrationServiceTests
{
    private const string IncorrectPackagingActivity = "IncorrectPackagingActivity";
    private const string IncorrectOrganisationTypeCode = "IncorrectOrganisationTypeCode";

    private Mock<IDequeueProvider> _dequeueProviderMock;
    private Mock<ICsvStreamParser> _csvStreamParserMock;
    private Mock<BlobClient> _blobClientMock;
    private Mock<BlobContainerClient> _blobContainerClientMock;
    private Mock<BlobServiceClient> _blobServiceClientMock;
    private Mock<ISubmissionApiClient> _submissionApiClientMock;
    private BlobReader _blobReader;
    private BlobQueueMessage? _blobQueueMessage;
    private RegistrationService _sut;

    [TestInitialize]
    public void Setup()
    {
        _dequeueProviderMock = new Mock<IDequeueProvider>();
        _csvStreamParserMock = new Mock<ICsvStreamParser>();

        IOptions<StorageAccountConfig> options = Options.Create<StorageAccountConfig>(new StorageAccountConfig
        {
            ConnectionString = "A",
            BlobContainerName = "B",
        });
        _submissionApiClientMock = new Mock<ISubmissionApiClient>();
        _blobClientMock = BlobStorageServiceTestsHelper.GetBlobClientMock();
        _blobContainerClientMock =
            BlobStorageServiceTestsHelper.GetBlobContainerClientMock(_blobClientMock.Object);
        _blobServiceClientMock =
            BlobStorageServiceTestsHelper.GetBlobServiceClientMock(_blobContainerClientMock.Object);
        _blobReader = new BlobReader(_blobServiceClientMock.Object, options);
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var factory = serviceProvider.GetService<ILoggerFactory>();
        var logger = factory.CreateLogger<RegistrationService>();
        _sut = new RegistrationService(_dequeueProviderMock.Object, _blobReader, _csvStreamParserMock.Object, _submissionApiClientMock.Object, logger);
    }

    [TestMethod]
    public void TestProcessServiceBusMessages_WhenSubmissionSubTypeIsNotCompanyDetails_ReturnsAndDoesNotProceed()
    {
        // ARRANGE
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.Brands;
        var csvDataRow = CSVRowTestHelper.GenerateCSVDataRowTestHelper(
            RequiredOrganisationTypeCodeForPartners.PAR.ToString(),
            IncorrectPackagingActivity);
        var csvDataRow2 =
            CSVRowTestHelper.GenerateCSVDataRowTestHelper(IncorrectOrganisationTypeCode, IncorrectPackagingActivity);
        _blobQueueMessage = new BlobQueueMessage()
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock.Setup(
            x => x.GetItemsFromCsvStream<CsvDataRow>(It.IsAny<MemoryStream>())).Returns(new List<CsvDataRow>
        {
            csvDataRow,
            csvDataRow2,
        });
        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        // ACT
        _sut.ProcessServiceBusMessage(serialisedMessage);

        // ASSERT
        _submissionApiClientMock.Verify(
            m =>
                m.SendEventRegistrationMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<RegistrationEvent>()),
            Times.Never);
        _csvStreamParserMock.Verify(
            m => m.GetItemsFromCsvStream<CsvDataRow>(It.IsAny<MemoryStream>()),
            Times.Never);
    }

    [TestMethod]
    public void TestProcessServiceBusMessages_WhenSubmissionSubTypeIsValid_ReturnsSuccessfully()
    {
        // ARRANGE
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.CompanyDetails;
        var csvDataRow = CSVRowTestHelper.GenerateCSVDataRowTestHelper(
            RequiredOrganisationTypeCodeForPartners.PAR.ToString(),
            IncorrectPackagingActivity);
        var csvDataRow2 =
            CSVRowTestHelper.GenerateCSVDataRowTestHelper(IncorrectOrganisationTypeCode, IncorrectPackagingActivity);
        _blobQueueMessage = new BlobQueueMessage()
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock.Setup(
            x => x.GetItemsFromCsvStream<CsvDataRow>(It.IsAny<MemoryStream>())).Returns(new List<CsvDataRow>
        {
            csvDataRow,
            csvDataRow2,
        });
        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        // ACT
        Func<Task> f = async () => await _sut.ProcessServiceBusMessage(serialisedMessage);

        // ASSERT
        f.Should().NotThrowAsync<Exception>();
        _submissionApiClientMock.Verify(
            m =>
                m.SendEventRegistrationMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<RegistrationEvent>()),
            Times.Once);
        _csvStreamParserMock.Verify(
            m => m.GetItemsFromCsvStream<CsvDataRow>(It.IsAny<MemoryStream>()),
            Times.Once);
    }

    [TestMethod]
    public void TestProcessServiceBusMessages_WhenCSVParseErrorIsThrown_ErrorRegistrationEventIsCreated()
    {
        // ARRANGE
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.CompanyDetails;
        var csvDataRow = CSVRowTestHelper.GenerateCSVDataRowTestHelper(
            RequiredOrganisationTypeCodeForPartners.PAR.ToString(),
            IncorrectPackagingActivity);
        var csvDataRow2 =
            CSVRowTestHelper.GenerateCSVDataRowTestHelper(IncorrectOrganisationTypeCode, IncorrectPackagingActivity);
        _blobQueueMessage = new BlobQueueMessage()
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock.Setup(
            x => x.GetItemsFromCsvStream<CsvDataRow>(It.IsAny<MemoryStream>())).Returns(new List<CsvDataRow>
        {
            csvDataRow,
            csvDataRow2,
        });
        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);
        _csvStreamParserMock.Setup(x => x.GetItemsFromCsvStream<CsvDataRow>(It.IsAny<MemoryStream>())).Throws<CsvParseException>();

        // ACT
        _sut.ProcessServiceBusMessage(serialisedMessage);

        // ASSERT
        _submissionApiClientMock.Verify(
            m =>
                m.SendEventRegistrationMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<RegistrationEvent>()),
            Times.Once);
    }
}