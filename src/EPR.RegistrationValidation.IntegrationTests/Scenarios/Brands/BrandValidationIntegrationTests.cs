namespace EPR.RegistrationValidation.IntegrationTests.Scenarios.Brands;

using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Enums;
using EPR.RegistrationValidation.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class BrandValidationIntegrationTests
{
    [TestMethod]
    [TestCategory("IntegrationTest")]
    public async Task ProcessServiceBusMessage_WhenBrandDoesNotMatchOrganisationFile_EmitsCrossFileError()
    {
        var harness = IntegrationTestHarness.Create(
            FeatureFlags.EnableRowValidation,
            FeatureFlags.EnableBrandPartnerDataRowValidation,
            FeatureFlags.EnableBrandPartnerCrossFileValidation);

        harness.BlobReader.AddOrUpdateBlob("brands.csv", CsvFixtureFactory.BrandsCsv("UNKNOWN-ORG", "UNKNOWN-SUB"));
        harness.BlobReader.AddOrUpdateBlob("organisation-file.csv", CsvFixtureFactory.ValidOrganisationCsv());

        var queueMessage = QueueMessageFixture.Build(
            blobName: "brands.csv",
            submissionSubType: nameof(SubmissionSubType.Brands),
            requiresRowValidation: true);

        await harness.RegistrationService.ProcessServiceBusMessage(queueMessage);

        harness.CapturedEvent.Should().NotBeNull();
        harness.CapturedEvent.Type.Should().Be(EventType.BrandValidation);
        harness.CapturedEvent.IsValid.Should().BeFalse();
        harness.CapturedEvent.Errors.Should().Contain(ErrorCodes.BrandDetailsNotMatchingOrganisation);
    }
}
