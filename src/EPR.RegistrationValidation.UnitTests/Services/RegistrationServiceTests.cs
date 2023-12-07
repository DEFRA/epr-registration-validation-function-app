namespace EPR.RegistrationValidation.UnitTests.Services;

using Application.Clients;
using Application.Exceptions;
using Application.Helpers;
using Application.Providers;
using Application.Readers;
using Application.Services;
using Data.Config;
using Data.Constants;
using Data.Enums;
using Data.Models;
using Data.Models.QueueMessages;
using Data.Models.SubmissionApi;
using FluentAssertions;
using Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using TestHelpers;

[TestClass]
public class RegistrationServiceTests
{
    private const string IncorrectPackagingActivity = "IncorrectPackagingActivity";
    private const string IncorrectOrganisationTypeCode = "IncorrectOrganisationTypeCode";
    private const string ContainerName = "BlobContainerName";

    private Mock<IDequeueProvider> _dequeueProviderMock;
    private Mock<ICsvStreamParser> _csvStreamParserMock;
    private Mock<ISubmissionApiClient> _submissionApiClientMock;
    private Mock<ILogger<RegistrationService>> _loggerMock;
    private BlobQueueMessage? _blobQueueMessage;
    private Mock<IFeatureManager> _featureManagerMock;
    private RegistrationService _sut;
    private Mock<IValidationService> _validationServiceMock;

    [TestInitialize]
    public void Setup()
    {
        IOptions<StorageAccountConfig> options = Options.Create(new StorageAccountConfig
        {
            ConnectionString = "A",
            BlobContainerName = ContainerName,
        });
        _featureManagerMock = new Mock<IFeatureManager>();
        _dequeueProviderMock = new Mock<IDequeueProvider>();
        _csvStreamParserMock = new Mock<ICsvStreamParser>();
        _submissionApiClientMock = new Mock<ISubmissionApiClient>();
        _validationServiceMock = new Mock<IValidationService>();
        var blobClientMock = BlobStorageServiceTestsHelper.GetBlobClientMock();
        var blobContainerClientMock = BlobStorageServiceTestsHelper.GetBlobContainerClientMock(blobClientMock.Object);
        var blobServiceClientMock = BlobStorageServiceTestsHelper.GetBlobServiceClientMock(blobContainerClientMock.Object);
        var blobReader = new BlobReader(blobServiceClientMock.Object, options);
        _loggerMock = new Mock<ILogger<RegistrationService>>();
        _sut = new RegistrationService(_dequeueProviderMock.Object, blobReader, _csvStreamParserMock.Object, _submissionApiClientMock.Object, options, _featureManagerMock.Object, _validationServiceMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public void TestProcessServiceBusMessages_WhenSubmissionSubTypeIsNotCompanyDetails_ReturnsAndDoesNotProceed()
    {
        // Arrange
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
            x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>())).ReturnsAsync(new List<OrganisationDataRow>
        {
            csvDataRow,
            csvDataRow2,
        });
        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        // Act
        _sut.ProcessServiceBusMessage(serialisedMessage);

        // Assert
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
            m => m.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()),
            Times.Never);
    }

    [TestMethod]
    public void TestProcessServiceBusMessages_WhenSubmissionSubTypeIsValid_ReturnsSuccessfully()
    {
        // Arrange
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
            x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>())).ReturnsAsync(new List<OrganisationDataRow>
        {
            csvDataRow,
            csvDataRow2,
        });
        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        // Act
        Func<Task> f = async () => await _sut.ProcessServiceBusMessage(serialisedMessage);

        // Assert
        f.Should().NotThrowAsync<Exception>();
        _submissionApiClientMock.Verify(
            m =>
                m.SendEventRegistrationMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<RegistrationEvent>(x =>
                        x.BlobContainerName == ContainerName
                        && x.BlobName == blobName
                        && !x.RequiresBrandsFile
                        && x.RequiresPartnershipsFile
                        && x.IsValid)),
            Times.Once);
        _csvStreamParserMock.Verify(
            m => m.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()),
            Times.Once);
    }

    [TestMethod]
    public void TestProcessServiceBusMessages_WhenCSVParseErrorIsThrown_ErrorRegistrationEventIsCreated()
    {
        // Arrange
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
        _blobQueueMessage = new BlobQueueMessage
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
            x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>())).ReturnsAsync(new List<OrganisationDataRow>
        {
            csvDataRow,
            csvDataRow2,
        });
        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);
        _csvStreamParserMock.Setup(x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>())).Throws<CsvParseException>();

        // Act
        _sut.ProcessServiceBusMessage(serialisedMessage);

        // Assert
        _submissionApiClientMock.Verify(
            m =>
                m.SendEventRegistrationMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<RegistrationEvent>(x =>
                        x.BlobContainerName == ContainerName
                        && x.BlobName == blobName
                        && !x.RequiresBrandsFile
                        && !x.RequiresPartnershipsFile
                        && !x.IsValid)),
            Times.Once);
    }

    [TestMethod]
    public void ProcessServiceBusMessage_AddsErrorAndLogs_WhenCsvFileIsEmpty()
    {
        // Arrange
        _blobQueueMessage = new BlobQueueMessage
        {
            UserId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid().ToString(),
            SubmissionId = Guid.NewGuid().ToString(),
            SubmissionSubType = SubmissionSubType.CompanyDetails.ToString(),
            BlobName = "some blob name",
        };

        var serializedQueueMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(serializedQueueMessage))
            .Returns(_blobQueueMessage);

        _csvStreamParserMock
            .Setup(x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()))
            .ReturnsAsync(new List<OrganisationDataRow>());

        // Act
        _sut.ProcessServiceBusMessage(serializedQueueMessage);

        // Assert
        _submissionApiClientMock.Verify(
            x => x.SendEventRegistrationMessage(
                _blobQueueMessage.OrganisationId,
                _blobQueueMessage.UserId,
                _blobQueueMessage.SubmissionId,
                _blobQueueMessage.UserType,
                It.Is<RegistrationEvent>(x =>
                    x.BlobContainerName == ContainerName
                    && x.BlobName == _blobQueueMessage.BlobName
                    && x.Errors.Count == 1 && x.Errors[0] == ErrorCodes.CsvFileEmptyErrorCode
                    && !x.RequiresBrandsFile
                    && !x.RequiresPartnershipsFile
                    && x.IsValid)),
            Times.Once);
        _loggerMock.VerifyLog(logger => logger.LogInformation("The CSV file for submission ID {submissionId} is empty", _blobQueueMessage.SubmissionId));
    }

    [TestMethod]
    public void TestProcessServiceBusMessages_WhenRowValidationFeatureIsEnableAndValidSubmissionType_CallsValidateAsyncServiceOnceAndReturnsSuccessfully()
    {
        // Arrange
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
            x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>())).ReturnsAsync(new List<OrganisationDataRow>
        {
            csvDataRow,
            csvDataRow2,
        });
        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.EnableRowValidation)).ReturnsAsync(true);

        // Act
        Func<Task> f = async () => await _sut.ProcessServiceBusMessage(serialisedMessage);

        // Assert
        f.Should().NotThrowAsync<Exception>();
        _submissionApiClientMock.Verify(
            m =>
                m.SendEventRegistrationMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<RegistrationEvent>(x =>
                        x.BlobContainerName == ContainerName
                        && x.BlobName == blobName
                        && !x.RequiresBrandsFile
                        && x.RequiresPartnershipsFile
                        && x.IsValid)),
            Times.Once);
        _csvStreamParserMock.Verify(
            m => m.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()),
            Times.Once);
        _validationServiceMock.Verify(v => v.ValidateAsync(It.IsAny<List<OrganisationDataRow>>()), Times.Once);
    }
}