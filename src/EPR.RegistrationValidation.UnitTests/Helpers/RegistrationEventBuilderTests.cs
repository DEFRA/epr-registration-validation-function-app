namespace EPR.RegistrationValidation.UnitTests.Helpers;

using Application.Helpers;
using Data.Enums;
using Data.Models;
using Data.Models.SubmissionApi;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

[TestClass]
public class RegistrationEventBuilderTests
{
    private const string IncorrectPackagingActivity = "IncorrectPackagingActivity";
    private const string IncorrectOrganisationTypeCode = "IncorrectOrganisationTypeCode";
    private const string BlobName = "BlobName";
    private const string ContainerName = "BlobContainerName";
    private const int ErrorLimit = 200;

    [TestMethod]
    [DataRow(RequiredOrganisationTypeCodeForPartners.PAR, RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.PAR, RequiredPackagingActivityForBrands.Secondary)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LLP, RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LLP, RequiredPackagingActivityForBrands.Secondary)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LPA, RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LPA, RequiredPackagingActivityForBrands.Secondary)]
    public void CreateValidationEvent_WhenCsvItemHasBrandsAndPartners_ReturnsCorrectRegistrationEvent(
        RequiredOrganisationTypeCodeForPartners organisationTypeCode,
        RequiredPackagingActivityForBrands packagingActivity)
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(organisationTypeCode.ToString(), packagingActivity.ToString());
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, validationErrors, BlobName, ContainerName, ErrorLimit);

        // Assert
        RegistrationEventTestHelper.AssertRegistrationValidationEvent(regEvent, validationErrors, true, true, BlobName);
    }

    [TestMethod]
    [DataRow(RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredPackagingActivityForBrands.Secondary)]
    public void CreateValidationEvent_WhenCsvItemHasBrandsAndNoPartners_ReturnsCorrectRegistrationEvent(RequiredPackagingActivityForBrands brands)
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var csvDataRow =
            CSVRowTestHelper.GenerateOrgCsvDataRow(IncorrectPackagingActivity, brands.ToString());
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, validationErrors, BlobName, ContainerName, ErrorLimit);

        // Assert
        RegistrationEventTestHelper.AssertRegistrationValidationEvent(regEvent, validationErrors, true, false, BlobName);
    }

    [TestMethod]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LLP)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.LPA)]
    [DataRow(RequiredOrganisationTypeCodeForPartners.PAR)]
    public void CreateValidationEvent_WhenCsvItemHasNoBrandsAndPartners_ReturnsCorrectRegistrationEvent(RequiredOrganisationTypeCodeForPartners partners)
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var csvDataRow =
            CSVRowTestHelper.GenerateOrgCsvDataRow(partners.ToString(), IncorrectOrganisationTypeCode);
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, validationErrors, BlobName, ContainerName, ErrorLimit);

        // Assert
        RegistrationEventTestHelper.AssertRegistrationValidationEvent(regEvent, validationErrors, false, true, BlobName);
    }

    [TestMethod]
    public void CreateValidationEvent_WhenCsvItemHasNoBrandsAndNoPartners_ReturnsCorrectRegistrationEvent()
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(IncorrectOrganisationTypeCode, IncorrectPackagingActivity);

        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, validationErrors, BlobName, ContainerName, ErrorLimit);

        // Assert
        RegistrationEventTestHelper.AssertRegistrationValidationEvent(regEvent, validationErrors, false, false, BlobName);
    }

    [TestMethod]
    public void CreateValidationEvent_WhenHasMultipleLines_ReturnsCorrectRegistrationEvent()
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();
        var csvDataRow =
            CSVRowTestHelper.GenerateOrgCsvDataRow(IncorrectOrganisationTypeCode, RequiredPackagingActivityForBrands.Primary.ToString());
        var csvDataRow2 =
            CSVRowTestHelper.GenerateOrgCsvDataRow(RequiredOrganisationTypeCodeForPartners.LLP.ToString(), IncorrectPackagingActivity);
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
            csvDataRow2,
        };

        // Act
        var regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, validationErrors, BlobName, ContainerName, ErrorLimit);

        // Assert
        RegistrationEventTestHelper.AssertRegistrationValidationEvent(regEvent, validationErrors, true, true, BlobName);
    }

    [TestMethod]
    public void CreateValidationEvent_WhenOrganisationDataHasValidationErrors_AndLessThanMaxNumberOfErrors_VerifyHasMaxRowErrorsIsFalse()
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();

        var numberBelowErrorLimit = ErrorLimit / 2;
        for (int i = 0; i < numberBelowErrorLimit; i++)
        {
            validationErrors.Add(new RegistrationValidationError
            {
                RowNumber = i,
                ColumnErrors = new List<ColumnValidationError> { new() { ErrorCode = "123" } },
            });
        }

        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(IncorrectOrganisationTypeCode, IncorrectPackagingActivity);

        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, validationErrors, BlobName, ContainerName, ErrorLimit);

        // Assert
        regEvent.Should().BeOfType<RegistrationValidationEvent>();
        regEvent.As<RegistrationValidationEvent>().RowErrorCount.Should().BeLessThan(ErrorLimit);
        regEvent.As<RegistrationValidationEvent>().HasMaxRowErrors.Should().BeFalse();
    }

    [TestMethod]
    public void CreateValidationEvent_WhenOrganisationDataHasValidationErrors_AndMaxNumberOfErrors_VerifyHasMaxRowErrorsIsTrue()
    {
        // Arrange
        var validationErrors = new List<RegistrationValidationError>();

        for (int i = 0; i < ErrorLimit; i++)
        {
            validationErrors.Add(new RegistrationValidationError
            {
                RowNumber = i,
                ColumnErrors = new List<ColumnValidationError> { new() { ErrorCode = "123" } },
            });
        }

        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(IncorrectOrganisationTypeCode, IncorrectPackagingActivity);

        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, validationErrors, BlobName, ContainerName, ErrorLimit);

        // Assert
        regEvent.Should().BeOfType<RegistrationValidationEvent>();
        regEvent.As<RegistrationValidationEvent>().RowErrorCount.Should().Be(ErrorLimit);
        regEvent.As<RegistrationValidationEvent>().HasMaxRowErrors.Should().BeTrue();
    }

    [TestMethod]
    public void CreateValidationEvent_WhenOrganisationDataHasZeroValidationErrors_VerifyRowErrorCountIsZero()
    {
        // Arrange
        var emptyValidationErrors = new List<RegistrationValidationError>();
        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(IncorrectOrganisationTypeCode, IncorrectPackagingActivity);

        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows, emptyValidationErrors, BlobName, ContainerName, ErrorLimit);

        // Assert
        regEvent.Should().BeOfType<RegistrationValidationEvent>();
        regEvent.As<RegistrationValidationEvent>().RowErrorCount.Should().Be(0);
    }

    [TestMethod]
    public void CreateValidationEvent_WhenOrganisationDataHasNullValidationErrors_VerifyRowErrorCountIsNull()
    {
        // Arrange
        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(IncorrectOrganisationTypeCode, IncorrectPackagingActivity);
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        // Act
        var regEvent = RegistrationEventBuilder.CreateValidationEvent(csvDataRows,  validationErrors: null, BlobName, ContainerName, ErrorLimit);

        // Assert
        regEvent.Should().BeOfType<RegistrationValidationEvent>();
        regEvent.As<RegistrationValidationEvent>().RowErrorCount.Should().BeNull();
    }

    [TestMethod]
    [DataRow(EventType.BrandValidation)]
    [DataRow(EventType.PartnerValidation)]
    [DataRow(EventType.Registration)]
    public void CreateValidationEvent_WithEventType_ReturnsCorrectTypeOfRegistrationEvent(EventType eventTypeValue)
    {
        // Arrange
        var eventType = eventTypeValue;
        var blobName = "BlobName";
        var blobContainerName = "BlobContainerName";
        string[] errors = { "813" };

        // Act
        ValidationEvent regEvent = RegistrationEventBuilder.CreateValidationEvent(eventType, blobName, blobContainerName, errors);

        // Assert
        RegistrationEventTestHelper.AssertValidationEvent(regEvent, eventTypeValue, blobName, blobContainerName, errors);
    }
}