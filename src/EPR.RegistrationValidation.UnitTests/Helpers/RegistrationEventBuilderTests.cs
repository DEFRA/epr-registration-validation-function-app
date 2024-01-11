namespace EPR.RegistrationValidation.UnitTests.Helpers;

using Application.Helpers;
using Data.Enums;
using Data.Models;
using Data.Models.SubmissionApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

[TestClass]
public class RegistrationEventBuilderTests
{
    private const string IncorrectPackagingActivity = "IncorrectPackagingActivity";
    private const string IncorrectOrganisationTypeCode = "IncorrectOrganisationTypeCode";
    private const string BlobName = "BlobName";
    private const string ContainerName = "BlobContainerName";

    [TestMethod]
    [DataRow(RequiredOrganisationTypeCodeForPartners.PAR, RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.PAR, RequiredPackagingActivityForBrands.Secondary)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LLP, RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LLP, RequiredPackagingActivityForBrands.Secondary)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LPA, RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LPA, RequiredPackagingActivityForBrands.Secondary)]
    public void TestBuildRegistrationValidationEvent_WhenCsvItemHasBrandsAndPartners_ReturnsCorrectRegistrationEvent(
        RequiredOrganisationTypeCodeForPartners organisationTypeCode,
        RequiredPackagingActivityForBrands packagingActivity)
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var csvDataRow = CSVRowTestHelper.GenerateCSVDataRowTestHelper(organisationTypeCode.ToString(), packagingActivity.ToString());
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, validationErrors, BlobName, ContainerName);

        // Assert
        RegistrationEventTestHelper.AssertRegistrationValidationEvent(regEvent, validationErrors, true, true, BlobName);
    }

    [TestMethod]
    [DataRow(RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredPackagingActivityForBrands.Secondary)]
    public void TestBuildRegistrationValidationEvent_WhenCsvItemHasBrandsAndNoPartners_ReturnsCorrectRegistrationEvent(RequiredPackagingActivityForBrands brands)
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var csvDataRow =
            CSVRowTestHelper.GenerateCSVDataRowTestHelper(IncorrectPackagingActivity, brands.ToString());
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        RegistrationValidationEvent regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, validationErrors,  BlobName, ContainerName) as RegistrationValidationEvent;

        // Assert
        RegistrationEventTestHelper.AssertRegistrationValidationEvent(regEvent, validationErrors, true, false, BlobName);
    }

    [TestMethod]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LLP)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LPA)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.PAR)]
    public void TestBuildRegistrationValidationEvent_WhenCsvItemHasNoBrandsAndPartners_ReturnsCorrectRegistrationEvent(RequiredOrganisationTypeCodeForPartners partners)
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var csvDataRow =
            CSVRowTestHelper.GenerateCSVDataRowTestHelper(partners.ToString(), IncorrectOrganisationTypeCode);
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        RegistrationValidationEvent regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, validationErrors,  BlobName, ContainerName) as RegistrationValidationEvent;

        // Assert
        RegistrationEventTestHelper.AssertRegistrationValidationEvent(regEvent, validationErrors, false, true, BlobName);
    }

    [TestMethod]
    public void TestBuildRegistrationValidationEvent_WhenCsvItemHasNoBrandsAndNoPartners_ReturnsCorrectRegistrationEvent()
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var csvDataRow = CSVRowTestHelper.GenerateCSVDataRowTestHelper(IncorrectOrganisationTypeCode, IncorrectPackagingActivity);

        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        RegistrationValidationEvent regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, validationErrors,  BlobName, ContainerName) as RegistrationValidationEvent;

        // Assert
        RegistrationEventTestHelper.AssertRegistrationValidationEvent(regEvent, validationErrors, false, false, BlobName);
    }

    [TestMethod]
    public void TestBuildRegistrationValidationEvent_WhenHasMultipleLines_ReturnsCorrectRegistrationEvent()
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var csvDataRow =
            CSVRowTestHelper.GenerateCSVDataRowTestHelper(IncorrectOrganisationTypeCode, RequiredPackagingActivityForBrands.Primary.ToString());
        var csvDataRow2 =
            CSVRowTestHelper.GenerateCSVDataRowTestHelper(RequiredOrganisationTypeCodeForPartners.LLP.ToString(), IncorrectPackagingActivity);
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
            csvDataRow2,
        };

        // Act
        RegistrationValidationEvent regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, validationErrors,  BlobName, ContainerName) as RegistrationValidationEvent;

        // Assert
        RegistrationEventTestHelper.AssertRegistrationValidationEvent(regEvent, validationErrors, true, true, BlobName);
    }

    [TestMethod]
    [DataRow(EventType.BrandValidation)]
    [DataRow(EventType.PartnerValidation)]
    public void TestBuildValidationEvent_WhenBrandOrPartner_ReturnsCorrectRegistrationEvent(EventType eventTypeValue)
    {
        // Arrange
        var eventType = eventTypeValue;
        var blobName = "BlobName";
        var blobContainerName = "BlobContainerName";
        string[] errors = { "812", "813" };

        // Act
        ValidationEvent regEvent = RegistrationEventBuilder.CreateValidationEvent(eventType,  blobName, blobContainerName, errors);

        // Assert
        RegistrationEventTestHelper.AssertValidationEvent(regEvent, eventTypeValue, blobName, blobContainerName, errors);
    }
}