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
        ValidationSettings validationSettings = new() { ErrorLimit = 200 };
        _sut = new RegistrationService(
            _dequeueProviderMock.Object,
            blobReader,
            _csvStreamParserMock.Object,
            _submissionApiClientMock.Object,
            options,
            _featureManagerMock.Object,
            _validationServiceMock.Object,
            _loggerMock.Object,
            Options.Create(validationSettings));
    }

    [TestMethod]
    public void ProcessServiceBusMessage_WhenSubmissionSubTypeIsNotCompanyDetails_DoesNotPostValidationEvent()
    {
        // Arrange
        _blobQueueMessage = new BlobQueueMessage()
        {
            UserId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid().ToString(),
            SubmissionId = Guid.NewGuid().ToString(),
            SubmissionSubType = "UnrecognisedSubType",
            BlobName = "test-blob",
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);

        // Act
        _sut.ProcessServiceBusMessage(JsonConvert.SerializeObject(_blobQueueMessage));

        // Assert
        _submissionApiClientMock.Verify(
            m =>
                m.SendEventRegistrationMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ValidationEvent>()),
            Times.Never);
        _csvStreamParserMock.Verify(
            m => m.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()),
            Times.Never);
    }

    [TestMethod]
    public void ProcessServiceBusMessage_WhenSubmissionSubTypeIsValid_ValidationEventIsCreated()
    {
        // Arrange
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.CompanyDetails;

        _blobQueueMessage = new BlobQueueMessage()
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
            RequiresRowValidation = true,
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock.Setup(
            x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>())).ReturnsAsync(new List<OrganisationDataRow>
        {
            CSVRowTestHelper.GenerateOrgCsvDataRow(),
            CSVRowTestHelper.GenerateOrgCsvDataRow(),
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
                    It.Is<RegistrationValidationEvent>(x =>
                        x.BlobContainerName == ContainerName
                        && x.BlobName == blobName
                        && !x.RequiresBrandsFile
                        && !x.RequiresPartnershipsFile
                        && x.IsValid)),
            Times.Once);
        _csvStreamParserMock.Verify(
            m => m.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()),
            Times.Once);
    }

    [TestMethod]
    public void ProcessServiceBusMessage_WhenCSVParseErrorIsThrown_ValidationErrorEventIsCreated()
    {
        // Arrange
        var blobName = "test";
        _blobQueueMessage = new BlobQueueMessage
        {
            UserId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid().ToString(),
            SubmissionId = Guid.NewGuid().ToString(),
            SubmissionSubType = SubmissionSubType.CompanyDetails.ToString(),
            BlobName = blobName,
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock
            .Setup(x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()))
            .Throws<CsvParseException>();

        // Act
        _sut.ProcessServiceBusMessage(JsonConvert.SerializeObject(_blobQueueMessage));

        // Assert
        _submissionApiClientMock.Verify(
            m =>
                m.SendEventRegistrationMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<ValidationEvent>(
                        x =>
                            x.BlobContainerName == ContainerName
                            && x.BlobName == blobName
                            && x.Errors.Count == 1 && x.Errors[0] == ErrorCodes.FileFormatInvalid
                            && !x.IsValid)),
            Times.Once);
    }

    [TestMethod]
    public void ProcessServiceBusMessage_WhenCsvHeaderExceptionIsThrown_ValidationErrorEventIsCreated()
    {
        // Arrange
        var blobName = "test";
        _blobQueueMessage = new BlobQueueMessage
        {
            UserId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid().ToString(),
            SubmissionId = Guid.NewGuid().ToString(),
            SubmissionSubType = SubmissionSubType.CompanyDetails.ToString(),
            BlobName = blobName,
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock
            .Setup(x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()))
            .Throws<CsvHeaderException>();

        // Act
        _sut.ProcessServiceBusMessage(JsonConvert.SerializeObject(_blobQueueMessage));

        // Assert
        _submissionApiClientMock.Verify(
            m =>
                m.SendEventRegistrationMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<ValidationEvent>(
                        x =>
                        x.BlobContainerName == ContainerName
                        && x.BlobName == blobName
                        && x.Errors.Count == 1 && x.Errors[0] == ErrorCodes.CsvFileInvalidHeaderErrorCode
                        && !x.IsValid)),
            Times.Once);
    }

    [TestMethod]
    public void ProcessServiceBusMessage_WhenCsvFileIsEmpty_AddsErrorAndLogs()
    {
        // Arrange
        _blobQueueMessage = new BlobQueueMessage
        {
            UserId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid().ToString(),
            SubmissionId = Guid.NewGuid().ToString(),
            SubmissionSubType = SubmissionSubType.CompanyDetails.ToString(),
            BlobName = "some blob name",
            RequiresRowValidation = true,
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
                It.Is<ValidationEvent>(e =>
                    e.BlobContainerName == ContainerName
                    && e.BlobName == _blobQueueMessage.BlobName
                    && e.Errors.Count == 1 && e.Errors[0] == ErrorCodes.CsvFileEmptyErrorCode
                    && !e.IsValid)),
            Times.Once);
        _loggerMock.VerifyLog(logger => logger.LogInformation("The CSV file for submission ID {SubmissionId} is empty", _blobQueueMessage.SubmissionId));
    }

    [TestMethod]
    public void ProcessServiceBusMessage_WhenSubmissionApiClientThrowsException_LogsError()
    {
        // Arrange
        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(RequiredOrganisationTypeCodeForPartners.PAR.ToString(), IncorrectPackagingActivity);
        _blobQueueMessage = new BlobQueueMessage
        {
            UserId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid().ToString(),
            SubmissionId = Guid.NewGuid().ToString(),
            SubmissionSubType = SubmissionSubType.CompanyDetails.ToString(),
            BlobName = "some blob name",
            RequiresRowValidation = true,
        };

        var serializedQueueMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(serializedQueueMessage))
            .Returns(_blobQueueMessage);

        _csvStreamParserMock
            .Setup(x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()))
            .ReturnsAsync(new List<OrganisationDataRow> { csvDataRow });

        _submissionApiClientMock.Setup(
                x => x.SendEventRegistrationMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ValidationEvent>()))
            .Throws<HttpRequestException>();

        // Act
        _sut.ProcessServiceBusMessage(serializedQueueMessage);

        // Assert
        _loggerMock.VerifyLog(logger => logger.LogError(It.IsAny<string>()));
    }

    [TestMethod]
    public void ProcessServiceBusMessage_WhenRowValidationFeatureIsDisabled_DoesNotCallValidateAsync()
    {
        // Arrange
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.CompanyDetails;
        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(
            RequiredOrganisationTypeCodeForPartners.PAR.ToString(),
            IncorrectPackagingActivity);
        var csvDataRow2 = CSVRowTestHelper.GenerateOrgCsvDataRow(IncorrectOrganisationTypeCode, IncorrectPackagingActivity);
        _blobQueueMessage = new BlobQueueMessage
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
            RequiresRowValidation = true,
        };

        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock
            .Setup(x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()))
            .ReturnsAsync(
                new List<OrganisationDataRow>
                {
                    csvDataRow,
                    csvDataRow2,
                });

        _validationServiceMock
            .Setup(x => x.ValidateOrganisationsAsync(It.IsAny<List<OrganisationDataRow>>()))
            .ReturnsAsync(new List<RegistrationValidationError>
            {
                It.IsAny<RegistrationValidationError>(),
            });

        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(false);
        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableOrganisationDataRowValidation))
            .ReturnsAsync(true);

        // Act
        Func<Task> f = async () => await _sut.ProcessServiceBusMessage(serialisedMessage);

        // Assert
        _validationServiceMock.Verify(
            v => v.ValidateOrganisationsAsync(It.IsAny<List<OrganisationDataRow>>()),
            Times.Never);
    }

    [TestMethod]
    public void ProcessServiceBusMessage_WhenRowValidationFeatureIsEnableAndValidSubmissionType_CallsValidateAsyncServiceOnceAndReturnsSuccessfully()
    {
        // Arrange
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.CompanyDetails;
        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(RequiredOrganisationTypeCodeForPartners.PAR.ToString(), IncorrectPackagingActivity);
        var csvDataRow2 = CSVRowTestHelper.GenerateOrgCsvDataRow(IncorrectOrganisationTypeCode, IncorrectPackagingActivity);
        _blobQueueMessage = new BlobQueueMessage
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
            RequiresRowValidation = true,
        };

        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);

        _csvStreamParserMock
            .Setup(x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()))
            .ReturnsAsync(
                new List<OrganisationDataRow>
                {
                    csvDataRow,
                    csvDataRow2,
                });

        _validationServiceMock
            .Setup(x => x.ValidateOrganisationsAsync(It.IsAny<List<OrganisationDataRow>>()))
            .ReturnsAsync(new List<RegistrationValidationError>
            {
                It.IsAny<RegistrationValidationError>(),
            });

        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(true);
        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableOrganisationDataRowValidation))
            .ReturnsAsync(true);

        // Act
        Func<Task> f = async () => await _sut.ProcessServiceBusMessage(serialisedMessage);

        // Assert
        f.Should().NotThrowAsync<Exception>();
        _validationServiceMock.Verify(
            v => v.ValidateOrganisationsAsync(It.IsAny<List<OrganisationDataRow>>()),
            Times.Once);
    }

    [TestMethod]
    public void ProcessServiceBusMessage_WhenRowValidationFeatureIsEnabled_AndRequiresRowValidationIsFalse_DoesNotCallValidateAsync()
    {
        // Arrange
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.CompanyDetails;
        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(RequiredOrganisationTypeCodeForPartners.PAR.ToString(), IncorrectPackagingActivity);
        _blobQueueMessage = new BlobQueueMessage
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
            RequiresRowValidation = false,
        };

        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);

        _csvStreamParserMock
            .Setup(x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()))
            .ReturnsAsync(
                new List<OrganisationDataRow>
                {
                    csvDataRow,
                });

        _validationServiceMock
            .Setup(x => x.ValidateOrganisationsAsync(It.IsAny<List<OrganisationDataRow>>()))
            .ReturnsAsync(new List<RegistrationValidationError>
            {
                It.IsAny<RegistrationValidationError>(),
            });

        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(true);
        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableOrganisationDataRowValidation))
            .ReturnsAsync(true);

        // Act
        Func<Task> f = async () => await _sut.ProcessServiceBusMessage(serialisedMessage);

        // Assert
        f.Should().NotThrowAsync<Exception>();
        _validationServiceMock.Verify(
            v => v.ValidateOrganisationsAsync(It.IsAny<List<OrganisationDataRow>>()),
            Times.Never);
    }

    [TestMethod]
    public void ProcessServiceBusMessage_WithBrandValidation_WhenRowValidationFeatureIsDisabled_DoesNotCallValidateAsync()
    {
        // Arrange
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.Brands;
        var csvDataRow = CSVRowTestHelper.GenerateBrandCsvDataRow();
        var csvDataRow2 = CSVRowTestHelper.GenerateBrandCsvDataRow();
        _blobQueueMessage = new BlobQueueMessage
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
            RequiresRowValidation = true,
        };

        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock
            .Setup(x => x.GetItemsFromCsvStreamAsync<BrandDataRow>(It.IsAny<MemoryStream>()))
            .ReturnsAsync(
                new List<BrandDataRow>
                {
                    csvDataRow,
                    csvDataRow2,
                });

        _validationServiceMock
            .Setup(x => x.ValidateAppendedFileAsync(It.IsAny<List<BrandDataRow>>()))
            .ReturnsAsync(new List<string>());

        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(false);
        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableBrandPartnerDataRowValidation))
            .ReturnsAsync(true);

        // Act
        Func<Task> f = async () => await _sut.ProcessServiceBusMessage(serialisedMessage);

        // Assert
        f.Should().NotThrowAsync<Exception>();
        _validationServiceMock.Verify(
            v => v.ValidateAppendedFileAsync(It.IsAny<List<BrandDataRow>>()),
            Times.Never);
    }

    [TestMethod]
    [DataRow(false, false, 0, DisplayName = "WhenDoesNotRequireRowValidation_AndValidationFlagDisabled_DoesNotSendValidationEvent")]
    [DataRow(false, true, 0, DisplayName = "WhenDoesNotRequireRowValidation_AndValidationFlagEnabled_DoesNotSendValidationEvent")]
    [DataRow(true, false, 1, DisplayName = "WhenRequiresRowValidation_AndValidationFlagDisabled_SendsValidationEvent")]
    [DataRow(true, true, 1, DisplayName = "WhenRequiresRowValidation_AndValidationFlagEnabled_SendsValidationEvent")]
    public void ProcessServiceBusMessage_RequiresRowValidationFlag_ValidateEventSent(
        bool requiresRowValidation,
        bool rowValidationEnabled,
        int timesCalled)
    {
        // Arrange
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.Brands;
        var csvDataRow = CSVRowTestHelper.GenerateBrandCsvDataRow();
        var csvDataRow2 = CSVRowTestHelper.GenerateBrandCsvDataRow();
        _blobQueueMessage = new BlobQueueMessage()
        {
            UserId = userId,
            OrganisationId = organisationId,
            UserType = "Producer",
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
            RequiresRowValidation = requiresRowValidation,
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock.Setup(
            x => x.GetItemsFromCsvStreamAsync<BrandDataRow>(It.IsAny<MemoryStream>())).ReturnsAsync(new List<BrandDataRow>
        {
            csvDataRow,
            csvDataRow2,
        });
        _validationServiceMock
            .Setup(x => x.ValidateAppendedFileAsync(It.IsAny<List<BrandDataRow>>()))
            .ReturnsAsync(new List<string>());

        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(rowValidationEnabled);

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
                    It.IsAny<ValidationEvent>()),
            Times.Exactly(timesCalled));
    }

    [TestMethod]
    [DataRow(true, 1, DisplayName = "Validation_WhenBrandPartnerRowValidationEnabled_CallsValidationService")]
    [DataRow(false, 0, DisplayName = "Validation_WhenBrandPartnerRowValidationDisabled_DoesNotCallValidationService")]
    public void ProcessServiceBusMessage_PartnerValidationFeature_VerifyValidateServiceCalls(bool flagEnabled, int timesCalled)
    {
        // Arrange
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.Partnerships;
        var csvDataRow = CSVRowTestHelper.GeneratePartnershipCsvDataRow();
        var csvDataRow2 = CSVRowTestHelper.GeneratePartnershipCsvDataRow();
        _blobQueueMessage = new BlobQueueMessage()
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
            RequiresRowValidation = true,
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock.Setup(
            x => x.GetItemsFromCsvStreamAsync<PartnersDataRow>(It.IsAny<MemoryStream>()))
            .ReturnsAsync(new List<PartnersDataRow>
            {
                csvDataRow,
                csvDataRow2,
            });
        _validationServiceMock
            .Setup(x => x.ValidateAppendedFileAsync(It.IsAny<List<PartnersDataRow>>()))
            .ReturnsAsync(new List<string>());

        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(true);
        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableBrandPartnerDataRowValidation))
            .ReturnsAsync(flagEnabled);

        // Act
        Func<Task> f = async () => await _sut.ProcessServiceBusMessage(serialisedMessage);

        // Assert
        f.Should().NotThrowAsync<Exception>();
        _validationServiceMock.Verify(v => v.ValidateAppendedFileAsync(It.IsAny<List<PartnersDataRow>>()), Times.Exactly(timesCalled));
    }

    [TestMethod]
    public async Task ProcessServiceBusMessage_WhenRequiresRowValidation_AndBrandPartnerRowValidationDisabled_AndCsvHeaderException_SendsIsValidEvent()
    {
        // Arrange
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.Partnerships;
        _blobQueueMessage = new BlobQueueMessage()
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
            RequiresRowValidation = true,
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock.Setup(
            x => x.GetItemsFromCsvStreamAsync<PartnersDataRow>(It.IsAny<MemoryStream>()))
            .Throws<CsvHeaderException>();

        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(true);
        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableBrandPartnerDataRowValidation))
            .ReturnsAsync(false);

        // Act
        await _sut.ProcessServiceBusMessage(serialisedMessage);

        // Assert
        _submissionApiClientMock.Verify(
            m =>
                m.SendEventRegistrationMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<ValidationEvent>(x => x.IsValid)),
            Times.Exactly(1));
    }

    [TestMethod]
    public void ProcessServiceBusMessage_WithBrandValidation_WhenEmptyFile_ValidationErrorEventIsCreated()
    {
        // Arrange
        var blobName = "test";
        _blobQueueMessage = new BlobQueueMessage
        {
            UserId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid().ToString(),
            SubmissionId = Guid.NewGuid().ToString(),
            SubmissionSubType = SubmissionSubType.Brands.ToString(),
            BlobName = blobName,
            RequiresRowValidation = true,
        };

        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock
            .Setup(x => x.GetItemsFromCsvStreamAsync<BrandDataRow>(It.IsAny<MemoryStream>()))
            .ReturnsAsync(new List<BrandDataRow>());
        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(true);
        _featureManagerMock
            .Setup(f => f.IsEnabledAsync(FeatureFlags.EnableBrandPartnerDataRowValidation))
            .ReturnsAsync(true);

        // Act
        Func<Task> f = async () => await _sut.ProcessServiceBusMessage(JsonConvert.SerializeObject(_blobQueueMessage));

        // Assert
        f.Should().NotThrowAsync<Exception>();
        _submissionApiClientMock.Verify(
            m =>
                m.SendEventRegistrationMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<ValidationEvent>(
                        x =>
                            x.BlobContainerName == ContainerName
                            && x.BlobName == blobName
                            && x.Errors.Count == 1 && x.Errors[0] == ErrorCodes.CsvFileEmptyErrorCode
                            && !x.IsValid)),
            Times.Once);
    }

    [TestMethod]
    public void ProcessServiceBusMessage_BrandsWhenThereIsAnError_CallsValidateEventWithErrorExpected()
    {
        // Arrange
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.Brands;
        var csvDataRow = CSVRowTestHelper.GenerateBrandCsvDataRow();
        var csvDataRow2 = CSVRowTestHelper.GenerateBrandCsvDataRow();
        _blobQueueMessage = new BlobQueueMessage()
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
            RequiresRowValidation = true,
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock.Setup(
            x => x.GetItemsFromCsvStreamAsync<BrandDataRow>(It.IsAny<MemoryStream>())).ReturnsAsync(new List<BrandDataRow>
        {
            csvDataRow,
            csvDataRow2,
        });
        _validationServiceMock.Setup(x => x.ValidateAppendedFileAsync(It.IsAny<List<BrandDataRow>>()))
            .ReturnsAsync(new List<string>
            {
                It.IsAny<string>(),
            });
        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.EnableRowValidation)).ReturnsAsync(true);
        _featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.EnableBrandPartnerDataRowValidation)).ReturnsAsync(true);

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
                    It.Is<ValidationEvent>(x =>
                        x.Type == EventType.BrandValidation
                        && x.Errors.Count == 1
                        && x.BlobContainerName == ContainerName
                        && x.BlobName == blobName
                        && !x.IsValid)),
            Times.Once);
        _csvStreamParserMock.Verify(
            m => m.GetItemsFromCsvStreamAsync<BrandDataRow>(It.IsAny<MemoryStream>()),
            Times.Once);
        _validationServiceMock.Verify(v => v.ValidateAppendedFileAsync(It.IsAny<List<BrandDataRow>>()), Times.Once);
    }

    [TestMethod]
    public void ProcessServiceBusMessage_PartnersWhenThereIsAnError_CallsValidateEventWithErrorExpected()
    {
        // Arrange
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.Partnerships;
        var csvDataRow = CSVRowTestHelper.GeneratePartnershipCsvDataRow();
        var csvDataRow2 = CSVRowTestHelper.GeneratePartnershipCsvDataRow();
        _blobQueueMessage = new BlobQueueMessage()
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
            RequiresRowValidation = true,
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock.Setup(
            x => x.GetItemsFromCsvStreamAsync<PartnersDataRow>(It.IsAny<MemoryStream>())).ReturnsAsync(new List<PartnersDataRow>
        {
            csvDataRow,
            csvDataRow2,
        });
        _validationServiceMock.Setup(x => x.ValidateAppendedFileAsync(It.IsAny<List<PartnersDataRow>>()))
            .ReturnsAsync(new List<string>
            {
                It.IsAny<string>(),
            });
        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.EnableRowValidation)).ReturnsAsync(true);
        _featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.EnableBrandPartnerDataRowValidation)).ReturnsAsync(true);

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
                    It.Is<ValidationEvent>(x =>
                        x.Type == EventType.PartnerValidation
                        && x.Errors.Count == 1
                        && x.BlobContainerName == ContainerName
                        && x.BlobName == blobName
                        && !x.IsValid)),
            Times.Once);
        _csvStreamParserMock.Verify(
            m => m.GetItemsFromCsvStreamAsync<PartnersDataRow>(It.IsAny<MemoryStream>()),
            Times.Once);
        _validationServiceMock.Verify(v => v.ValidateAppendedFileAsync(It.IsAny<List<PartnersDataRow>>()), Times.Once);
    }

    [TestMethod]
    public void ProcessServiceBusMessage_WhenOrgCSVFileColumnHasTooManyCharacters_CallsValidateEventWithErrorExpected()
    {
        // Arrange
        var blobName = "test";
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.CompanyDetails;

        var csvDataRow =
            CSVRowTestHelper.GenerateOrgCsvDataRowWithTooManyCharacters(
                IncorrectOrganisationTypeCode,
                IncorrectPackagingActivity);
        _blobQueueMessage = new BlobQueueMessage
        {
            UserId = userId,
            OrganisationId = organisationId,
            SubmissionId = submissionId,
            SubmissionSubType = submissionSubType.ToString(),
            BlobName = blobName,
            RequiresRowValidation = true,
        };
        _dequeueProviderMock
            .Setup(x => x.GetMessageFromJson<BlobQueueMessage>(It.IsAny<string>()))
            .Returns(_blobQueueMessage);
        _csvStreamParserMock
            .Setup(x => x.GetItemsFromCsvStreamAsync<OrganisationDataRow>(It.IsAny<MemoryStream>()))
            .ReturnsAsync(
                new List<OrganisationDataRow>
                {
                    csvDataRow,
                });

        _validationServiceMock
            .Setup(x => x.IsColumnLengthExceeded(It.IsAny<List<OrganisationDataRow>>())).Returns(true);

        var serialisedMessage = JsonConvert.SerializeObject(_blobQueueMessage);

        _featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.EnableRowValidation)).ReturnsAsync(true);
        _featureManagerMock.Setup(f => f.IsEnabledAsync(FeatureFlags.EnableOrganisationDataRowValidation))
            .ReturnsAsync(true);

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
                    It.Is<ValidationEvent>(x =>
                        x.Type == EventType.Registration
                        && x.Errors.Count == 1 && x.Errors[0] == ErrorCodes.CharacterLengthExceeded
                        && x.BlobContainerName == ContainerName
                        && x.BlobName == blobName
                        && !x.IsValid)),
            Times.Once);
    }
}