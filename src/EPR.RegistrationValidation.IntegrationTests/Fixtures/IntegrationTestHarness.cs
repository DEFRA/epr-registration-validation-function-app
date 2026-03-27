namespace EPR.RegistrationValidation.IntegrationTests.Fixtures;

using EPR.RegistrationValidation.Application.Helpers;
using EPR.RegistrationValidation.Application.Providers;
using EPR.RegistrationValidation.Application.Services;
using EPR.RegistrationValidation.Application.Services.Subsidiary;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Config;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models.CompanyDetailsApi;
using EPR.RegistrationValidation.Data.Models.SubmissionApi;
using EPR.RegistrationValidation.IntegrationTests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

public class IntegrationTestHarness
{
    private IntegrationTestHarness(
        RegistrationService registrationService,
        InMemoryBlobReader blobReader,
        InMemorySubmissionApiClient submissionApiClient,
        InMemoryCompanyDetailsApiClient companyDetailsApiClient)
    {
        RegistrationService = registrationService;
        BlobReader = blobReader;
        SubmissionApiClient = submissionApiClient;
        CompanyDetailsApiClient = companyDetailsApiClient;
    }

    public RegistrationService RegistrationService { get; }

    public InMemoryBlobReader BlobReader { get; }

    public InMemorySubmissionApiClient SubmissionApiClient { get; }

    public InMemoryCompanyDetailsApiClient CompanyDetailsApiClient { get; }

    public ValidationEvent CapturedEvent => SubmissionApiClient.CapturedValidationEvent;

    public static IntegrationTestHarness Create(params string[] enabledFlags) =>
        BuildHarness(new ValidationSettings { ErrorLimit = 200 }, enabledFlags);

    public static IntegrationTestHarness CreateWithClosedLoopFromYear(int closedLoopRegistrationFromYear, params string[] enabledFlags) =>
        BuildHarness(new ValidationSettings { ErrorLimit = 200, ClosedLoopRegistrationFromYear = closedLoopRegistrationFromYear }, enabledFlags);

    private static IntegrationTestHarness BuildHarness(ValidationSettings validationSettings, string[] enabledFlags)
    {
        var featureManager = new InMemoryFeatureManager(
            new[]
            {
                FeatureFlags.EnableOrganisationSizeFieldValidation,
                FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns,
                FeatureFlags.EnableStatusCodeColumn,
                FeatureFlags.EnableAdditionalValidationForJoinerLeaverColumns,
                FeatureFlags.EnableLeaverCodeValidation,
            }.Concat(enabledFlags ?? Array.Empty<string>()));

        var blobReader = new InMemoryBlobReader();
        var submissionApiClient = new InMemorySubmissionApiClient();
        var companyDetailsApiClient = new InMemoryCompanyDetailsApiClient
        {
            ProducerDetailsResult = new CompanyDetailsDataResult
            {
                Organisations = new List<CompanyDetailsDataItem>
                {
                    new() { ReferenceNumber = "145879", CompaniesHouseNumber = "11893759" },
                    new() { ReferenceNumber = "213458", CompaniesHouseNumber = "8974610" },
                },
            },
        };

        var rowValidators = new RowValidators(
            new OrganisationDataRowValidator(featureManager),
            new OrganisationDataRowWarningValidator(),
            new BrandDataRowValidator(),
            new PartnerDataRowValidator());

        var validationService = new ValidationService(
            rowValidators,
            new ColumnMetaDataProvider(featureManager),
            new ValidationConfig
            {
                ValidationSettings = Options.Create(validationSettings),
                RegistrationSettings = Options.Create(new RegistrationSettings
                {
                    SubmissionPeriod2026 = "January to June 2026",
                    SmallProducersRegStartTime2026 = DateTime.UtcNow.Date.AddDays(-10),
                    SmallProducersRegEndTime2026 = DateTime.UtcNow.Date.AddDays(10),
                }),
            },
            new ApiClients
            {
                CompanyDetailsApiClient = companyDetailsApiClient,
                SubmissionApiClient = submissionApiClient,
            },
            NullLogger<ValidationService>.Instance,
            featureManager,
            new SubsidiaryDetailsRequestBuilder());

        var registrationService = new RegistrationService(
            new DequeueProvider(),
            blobReader,
            new CsvStreamParser(new ColumnMetaDataProvider(featureManager), featureManager),
            submissionApiClient,
            Options.Create(new StorageAccountConfig
            {
                BlobContainerName = "integration-container",
                ConnectionString = "UseDevelopmentStorage=true",
            }),
            featureManager,
            validationService,
            NullLogger<RegistrationService>.Instance,
            Options.Create(validationSettings));

        return new IntegrationTestHarness(registrationService, blobReader, submissionApiClient, companyDetailsApiClient);
    }
}
