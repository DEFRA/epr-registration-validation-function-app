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
    public void TestBuildRegistrationEvent_WhenCsvItemHasBrandsAndPartners_ReturnsCorrectRegistrationEvent(
        RequiredOrganisationTypeCodeForPartners organisationTypeCode,
        RequiredPackagingActivityForBrands packagingActivity)
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var errors = new List<string>();
        var csvDataRow = CSVRowTestHelper.GenerateCSVDataRowTestHelper(organisationTypeCode.ToString(), packagingActivity.ToString());
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.BuildRegistrationEvent(csvDataRows, errors, validationErrors, BlobName, ContainerName);

        // Assert
        RegistrationEventTestHelper.AssertRegEvent(regEvent, errors, validationErrors, true, true, BlobName);
    }

    [TestMethod]
    [DataRow(RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredPackagingActivityForBrands.Secondary)]
    public void TestBuildRegistrationEvent_WhenCsvItemHasBrandsAndNoPartners_ReturnsCorrectRegistrationEvent(RequiredPackagingActivityForBrands brands)
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var errors = new List<string>();
        var csvDataRow =
            CSVRowTestHelper.GenerateCSVDataRowTestHelper(IncorrectPackagingActivity, brands.ToString());
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.BuildRegistrationEvent(csvDataRows, errors, validationErrors,  BlobName, ContainerName);

        // Assert
        RegistrationEventTestHelper.AssertRegEvent(regEvent, errors, validationErrors, true, false, BlobName);
    }

    [TestMethod]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LLP)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LPA)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.PAR)]
    public void TestBuildRegistrationEvent_WhenCsvItemHasNoBrandsAndPartners_ReturnsCorrectRegistrationEvent(RequiredOrganisationTypeCodeForPartners partners)
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var errors = new List<string>();
        var csvDataRow =
            CSVRowTestHelper.GenerateCSVDataRowTestHelper(partners.ToString(), IncorrectOrganisationTypeCode);
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.BuildRegistrationEvent(csvDataRows, errors, validationErrors, BlobName, ContainerName);

        // Assert
        RegistrationEventTestHelper.AssertRegEvent(regEvent, errors, validationErrors, false, true, BlobName);
    }

    [TestMethod]
    public void TestBuildRegistrationEvent_WhenCsvItemHasNoBrandsAndNoPartners_ReturnsCorrectRegistrationEvent()
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var errors = new List<string>();
        var csvDataRow = CSVRowTestHelper.GenerateCSVDataRowTestHelper(IncorrectOrganisationTypeCode, IncorrectPackagingActivity);

        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.BuildRegistrationEvent(csvDataRows, errors,  validationErrors, BlobName, ContainerName);

        // Assert
        RegistrationEventTestHelper.AssertRegEvent(regEvent, errors, validationErrors, false, false, BlobName);
    }

    [TestMethod]
    public void TestBuildRegistrationEvent_WhenHasMultipleLines_ReturnsCorrectRegistrationEvent()
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var errors = new List<string>();
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
        var regEvent = RegistrationEventBuilder.BuildRegistrationEvent(csvDataRows, errors, validationErrors, BlobName, ContainerName);

        // Assert
        RegistrationEventTestHelper.AssertRegEvent(regEvent, errors, validationErrors, true, true, BlobName);
    }

    [TestMethod]
    public void TestBuildErrorRegistrationEvent_WhenErrorsParameterIsPassed_ReturnsCorrectRegistrationEvent()
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var errors = new List<string>
        {
            "99",
        };
        var csvDataRow = CSVRowTestHelper.GenerateCSVDataRowTestHelper(
            RequiredOrganisationTypeCodeForPartners.LLP.ToString(),
            RequiredPackagingActivityForBrands.Primary.ToString());
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.BuildRegistrationEvent(csvDataRows, errors, validationErrors, BlobName, ContainerName);

        // Assert
        RegistrationEventTestHelper.AssertRegEvent(regEvent, errors, validationErrors, true, true, BlobName);
    }
}