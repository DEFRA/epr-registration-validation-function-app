namespace EPR.RegistrationValidation.IntegrationTests.Scenarios.Partnerships;

using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Enums;
using EPR.RegistrationValidation.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PartnershipValidationIntegrationTests
{
    [TestMethod]
    [TestProperty("Category", "IntegrationTest")]
    public async Task ProcessServiceBusMessage_WhenPartnerDoesNotMatchOrganisationFile_EmitsCrossFileError()
    {
        var harness = IntegrationTestHarness.Create(
            FeatureFlags.EnableRowValidation,
            FeatureFlags.EnableBrandPartnerDataRowValidation,
            FeatureFlags.EnableBrandPartnerCrossFileValidation);

        harness.BlobReader.AddOrUpdateBlob("partners.csv", CsvFixtureFactory.PartnersCsv("UNKNOWN-ORG", "UNKNOWN-SUB"));
        harness.BlobReader.AddOrUpdateBlob("organisation-file.csv", CsvFixtureFactory.ValidOrganisationCsv());

        var queueMessage = QueueMessageFixture.Build(
            blobName: "partners.csv",
            submissionSubType: nameof(SubmissionSubType.Partnerships),
            requiresRowValidation: true);

        await harness.RegistrationService.ProcessServiceBusMessage(queueMessage);

        harness.CapturedEvent.Should().NotBeNull();
        harness.CapturedEvent.Type.Should().Be(EventType.PartnerValidation);
        harness.CapturedEvent.IsValid.Should().BeFalse();
        harness.CapturedEvent.Errors.Should().Contain(ErrorCodes.PartnerDetailsNotMatchingOrganisation);
    }
}
