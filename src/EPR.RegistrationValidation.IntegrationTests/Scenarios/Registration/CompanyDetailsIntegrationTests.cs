namespace EPR.RegistrationValidation.IntegrationTests.Scenarios.Registration;

using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Enums;
using EPR.RegistrationValidation.Data.Models.SubmissionApi;
using EPR.RegistrationValidation.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CompanyDetailsIntegrationTests
{
    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task ProcessServiceBusMessage_WhenCompanyDetailsCsvIsValid_EmitsValidRegistrationEvent()
    {
        var harness = IntegrationTestHarness.Create(
            FeatureFlags.EnableRowValidation,
            FeatureFlags.EnableOrganisationDataRowValidation);
        harness.BlobReader.AddOrUpdateBlob("company-details.csv", CsvFixtureFactory.ValidOrganisationCsv());

        var queueMessage = QueueMessageFixture.Build(
            blobName: "company-details.csv",
            submissionSubType: nameof(SubmissionSubType.CompanyDetails),
            requiresRowValidation: false);

        await harness.RegistrationService.ProcessServiceBusMessage(queueMessage);

        harness.CapturedEvent.Should().NotBeNull();
        harness.CapturedEvent.Type.Should().Be(EventType.Registration);
        harness.CapturedEvent.BlobName.Should().Be("company-details.csv");
        harness.CapturedEvent.BlobContainerName.Should().Be("integration-container");
        harness.CapturedEvent.IsValid.Should().BeTrue();
        harness.CapturedEvent.Errors.Should().BeNull();
    }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task ProcessServiceBusMessage_WhenCompanyDetailsHeadersAreInvalid_EmitsHeaderError()
    {
        var harness = IntegrationTestHarness.Create(
            FeatureFlags.EnableRowValidation,
            FeatureFlags.EnableOrganisationDataRowValidation);
        harness.BlobReader.AddOrUpdateBlob("invalid-header.csv", CsvFixtureFactory.InvalidOrganisationHeaderCsv());

        var queueMessage = QueueMessageFixture.Build(
            blobName: "invalid-header.csv",
            submissionSubType: nameof(SubmissionSubType.CompanyDetails),
            requiresRowValidation: true);

        await harness.RegistrationService.ProcessServiceBusMessage(queueMessage);

        harness.CapturedEvent.Should().NotBeNull();
        harness.CapturedEvent.Type.Should().Be(EventType.Registration);
        harness.CapturedEvent.IsValid.Should().BeFalse();
        var registrationEvent = harness.CapturedEvent as RegistrationValidationEvent;
        registrationEvent.Should().BeNull();
        harness.CapturedEvent.Errors.Should().ContainSingle().Which.Should().Be(ErrorCodes.CsvFileInvalidHeaderErrorCode);
    }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task ProcessServiceBusMessage_WhenOrganisationRowsContainErrors_EmitsRowValidationErrors()
    {
        var harness = IntegrationTestHarness.Create(
            FeatureFlags.EnableRowValidation,
            FeatureFlags.EnableOrganisationDataRowValidation);
        harness.BlobReader.AddOrUpdateBlob("row-errors.csv", CsvFixtureFactory.OrganisationCsvWithMissingOrganisationId());

        var queueMessage = QueueMessageFixture.Build(
            blobName: "row-errors.csv",
            submissionSubType: nameof(SubmissionSubType.CompanyDetails),
            requiresRowValidation: true);

        await harness.RegistrationService.ProcessServiceBusMessage(queueMessage);

        harness.CapturedEvent.Should().NotBeNull();
        harness.CapturedEvent.Type.Should().Be(EventType.Registration);
        harness.CapturedEvent.IsValid.Should().BeFalse();
        var registrationEvent = harness.CapturedEvent as RegistrationValidationEvent;
        registrationEvent.Should().NotBeNull();
        registrationEvent.ValidationErrors.Should().NotBeEmpty();
        registrationEvent.ValidationErrors
            .SelectMany(x => x.ColumnErrors)
            .Select(x => x.ErrorCode)
            .Should()
            .Contain(ErrorCodes.MissingOrganisationId);
    }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task ProcessServiceBusMessage_WhenClosedLoopRegistrationColumnIsAbsent_DoesNotEmitClosedLoopError()
    {
        var harness = IntegrationTestHarness.Create(
            FeatureFlags.EnableRowValidation,
            FeatureFlags.EnableOrganisationDataRowValidation);
        harness.BlobReader.AddOrUpdateBlob("no-closed-loop.csv", CsvFixtureFactory.ValidOrganisationCsv());

        var queueMessage = QueueMessageFixture.Build(
            blobName: "no-closed-loop.csv",
            submissionSubType: nameof(SubmissionSubType.CompanyDetails),
            requiresRowValidation: true);

        await harness.RegistrationService.ProcessServiceBusMessage(queueMessage);

        var registrationEvent = harness.CapturedEvent as RegistrationValidationEvent;
        registrationEvent.Should().NotBeNull();

        var allErrorCodes = registrationEvent.ValidationErrors
            .SelectMany(x => x.ColumnErrors)
            .Select(x => x.ErrorCode)
            .ToList();

        allErrorCodes.Should().NotContain(ErrorCodes.InvalidClosedLoopRegistrationValue);
    }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task ProcessServiceBusMessage_WhenClosedLoopRegistrationIsEmpty_EmitsClosedLoopError()
    {
        var harness = IntegrationTestHarness.Create(
            FeatureFlags.EnableRowValidation,
            FeatureFlags.EnableOrganisationDataRowValidation);
        harness.BlobReader.AddOrUpdateBlob(
            "closed-loop-empty.csv",
            CsvFixtureFactory.OrganisationCsvWithClosedLoopRegistrationEmptyValue());

        var queueMessage = QueueMessageFixture.Build(
            blobName: "closed-loop-empty.csv",
            submissionSubType: nameof(SubmissionSubType.CompanyDetails),
            requiresRowValidation: true);

        await harness.RegistrationService.ProcessServiceBusMessage(queueMessage);

        var registrationEvent = harness.CapturedEvent as RegistrationValidationEvent;
        registrationEvent.Should().NotBeNull();

        var allErrorCodes = registrationEvent.ValidationErrors
            .SelectMany(x => x.ColumnErrors)
            .Select(x => x.ErrorCode)
            .ToList();

        allErrorCodes.Should().Contain(ErrorCodes.InvalidClosedLoopRegistrationValue);
    }

    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task ProcessServiceBusMessage_WhenClosedLoopRegistrationIsInvalidValue_EmitsClosedLoopError()
    {
        var harness = IntegrationTestHarness.Create(
            FeatureFlags.EnableRowValidation,
            FeatureFlags.EnableOrganisationDataRowValidation);
        harness.BlobReader.AddOrUpdateBlob(
            "closed-loop-invalid.csv",
            CsvFixtureFactory.OrganisationCsvWithClosedLoopRegistrationValue("Maybe"));

        var queueMessage = QueueMessageFixture.Build(
            blobName: "closed-loop-invalid.csv",
            submissionSubType: nameof(SubmissionSubType.CompanyDetails),
            requiresRowValidation: true);

        await harness.RegistrationService.ProcessServiceBusMessage(queueMessage);

        var registrationEvent = harness.CapturedEvent as RegistrationValidationEvent;
        registrationEvent.Should().NotBeNull();

        var allErrorCodes = registrationEvent.ValidationErrors
            .SelectMany(x => x.ColumnErrors)
            .Select(x => x.ErrorCode)
            .ToList();

        allErrorCodes.Should().Contain(ErrorCodes.InvalidClosedLoopRegistrationValue);
    }
}
