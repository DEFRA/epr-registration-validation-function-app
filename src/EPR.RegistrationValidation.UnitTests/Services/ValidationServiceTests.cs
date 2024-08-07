﻿namespace EPR.RegistrationValidation.UnitTests.Services;

using EPR.RegistrationValidation.Application.Clients;
using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Helpers;
using EPR.RegistrationValidation.Application.Services;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Config;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.Data.Models.CompanyDetailsApi;
using EPR.RegistrationValidation.Data.Models.QueueMessages;
using EPR.RegistrationValidation.Data.Models.Services;
using EPR.RegistrationValidation.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class ValidationServiceTests
{
    private Mock<ICompanyDetailsApiClient> _companyDetailsApiClientMock;
    private Mock<ILogger<ValidationService>> _loggerMock;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<ValidationService>>();
    }

    [TestMethod]
    public async Task Validate_WithoutOrgId_ExpectOrgIdValidationError()
    {
        // Arrange
        const int expectedRow = 0;
        const int expectedColumnIndex = 0;
        const string expectedColumnName = "organisation_id";
        var service = CreateService();
        var dataRows = new List<OrganisationDataRow> { new() };
        var blobQueueMessage = new BlobQueueMessage();

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows, blobQueueMessage, false);

        // Assert
        var validationError = results.Find(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.MissingOrganisationId));
        validationError.Should().NotBeNull();
        validationError.RowNumber.Should().Be(expectedRow);
        validationError.ColumnErrors.First().ColumnIndex.Should().Be(expectedColumnIndex);
        validationError.ColumnErrors.First().ColumnName.Should().Be(expectedColumnName);
    }

    [TestMethod]
    public async Task Validate_WithOrgId_ExpectNoValidationErrors()
    {
        // Arrange
        var service = CreateService();
        var dataRows = new List<OrganisationDataRow>
        {
            new()
            {
                DefraId = "1234567890",
                OrganisationName = $"AAA ltd",
                HomeNationCode = "EN",
                PrimaryContactPersonLastName = "LName",
                PrimaryContactPersonFirstName = "Fname",
                PrimaryContactPersonEmail = "test@test.com",
                PrimaryContactPersonPhoneNumber = "01237946",
                PackagingActivitySO = "Primary",
                PackagingActivityHl = "Secondary",
                PackagingActivityPf = "Secondary",
                PackagingActivitySl = "Secondary",
                PackagingActivityIm = "No",
                PackagingActivityOm = "No",
                PackagingActivitySe = "Secondary",
                ProduceBlankPackagingFlag = "No",
                Turnover = $"99.99",
                ServiceOfNoticeAddressLine1 = "9 Surrey",
                ServiceOfNoticeAddressPostcode = "KT5 8JU",
                ServiceOfNoticeAddressPhoneNumber = "0123456789",
                AuditAddressLine1 = "10 Southcote",
                AuditAddressCountry = AuditingCountryCodes.England,
                AuditAddressPostcode = "KT5 9UW",
                PrincipalAddressLine1 = "Principal Address Line 1",
                PrincipalAddressPostcode = "Principal Address Postcode",
                PrincipalAddressPhoneNumber = "01237946",
                OrganisationTypeCode = UnIncorporationTypeCodes.SoleTrader,
            },
        };
        var blobQueueMessage = new BlobQueueMessage();

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows, blobQueueMessage, false);

        // Assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithManyValidRows_AndRowCountGreaterThanErrorLimit_ExpectNoValidationErrors()
    {
        // Arrange
        const int rowCount = 20;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount);
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });
        var blobQueueMessage = new BlobQueueMessage();

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows.ToList(), blobQueueMessage, false);

        // Assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithNoDuplicateOrganisationIdSubsidiaryId_ExpectNoValidationErrors()
    {
        // Arrange
        const int rowCount = 6;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgIdSubId(rowCount);
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });
        var blobQueueMessage = new BlobQueueMessage();

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows.ToList(), blobQueueMessage, false);

        // Assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithDuplicateOrganisationIdSubsidiaryId_ExpectValidationErrors()
    {
        // Arrange
        const int rowCount = 6;
        const int maxErrors = 20;
        var dataRows = RowDataTestHelper.GenerateDuplicateOrgIdSubId(rowCount);

        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });
        var blobQueueMessage = new BlobQueueMessage();

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows.ToList(), blobQueueMessage, false);

        // Assert
        var validationError = results.Find(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.DuplicateOrganisationIdSubsidiaryId));
        validationError.Should().NotBeNull();
    }

    [TestMethod]
    public async Task Validate_WithDuplicateRows_AndRowCountGreaterThanErrorLimit_ReturnsLimitedErrors()
    {
        // Arrange
        const int rowCount = 20;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateDuplicateOrgIdSubId(rowCount);
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });
        var blobQueueMessage = new BlobQueueMessage();

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows.ToList(), blobQueueMessage, false);

        // Assert
        results.Sum(x => x.ColumnErrors.Count).Should().Be(maxErrors);
    }

    [TestMethod]
    public async Task ValidateRowsAsync_WithInvalidRows_AndRowCountGreaterThanErrorLimit_ReturnsLimitedErrors()
    {
        // Arrange
        const int rowCount = 20;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateInvalidOrgs(rowCount);

        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = await service.ValidateRowsAsync(dataRows.ToList());

        // Assert
        results.TotalErrors.Should().Be(maxErrors);
    }

    [TestMethod]
    public async Task ValidateDuplicates_WithDuplicateRows_AndRowCountGreaterThanErrorLimit_ReturnsLimitedErrors()
    {
        // Arrange
        const int rowCount = 20;
        const int maxErrors = 10;
        const int initialTotalErrors = 5;
        const int expectedDuplicateErrors = 5;
        var dataRows = RowDataTestHelper.GenerateDuplicateOrgIdSubId(rowCount);

        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = service.ValidateDuplicates(dataRows.ToList(), initialTotalErrors);

        // Assert
        results.TotalErrors.Should().Be(maxErrors);
        results.ValidationErrors.SelectMany(x => x.ColumnErrors)
            .Count(x => x.ErrorCode == ErrorCodes.DuplicateOrganisationIdSubsidiaryId).Should()
            .Be(expectedDuplicateErrors);
    }

    [TestMethod]
    public async Task ValidateOrganisationSubType_WithNoSubOrganisationType_ReturnsRowError()
    {
        // Arrange
        const int rowCount = 20;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].OrganisationSubTypeCode = OrganisationSubTypeCodes.Licensor;
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = service.ValidateOrganisationSubType(dataRows.ToList(), 0);

        // Assert
        var validationError = results.ValidationErrors.Find(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.HeadOrganisationMissingSubOrganisation));
        validationError.Should().NotBeNull();
    }

    [TestMethod]
    public async Task ValidateOrganisationSubType_WithSubOrganisationTypeAndEmptySubsidiaryID_ReturnsRowError()
    {
        // Arrange
        const int rowCount = 20;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].OrganisationSubTypeCode = OrganisationSubTypeCodes.Licensor;
        dataRows[1].DefraId = dataRows[0].DefraId;
        dataRows[1].OrganisationSubTypeCode = OrganisationSubTypeCodes.Tenant;
        dataRows[1].SubsidiaryId = string.Empty;
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = service.ValidateOrganisationSubType(dataRows.ToList(), 0);

        // Assert
        var validationError = results.ValidationErrors.Find(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.HeadOrganisationMissingSubOrganisation));
        validationError.Should().NotBeNull();
    }

    [TestMethod]
    public async Task ValidateOrganisationSubType_WithSubOrganisationType_ExpectNoValidationErrors()
    {
        // Arrange
        const int rowCount = 20;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].OrganisationSubTypeCode = OrganisationSubTypeCodes.Licensor;
        dataRows[1].DefraId = dataRows[0].DefraId;
        dataRows[1].OrganisationSubTypeCode = OrganisationSubTypeCodes.Tenant;
        dataRows[1].SubsidiaryId = "54321";
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = service.ValidateOrganisationSubType(dataRows.ToList(), 0);

        // Assert
        var validationError = results.ValidationErrors.Find(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.HeadOrganisationMissingSubOrganisation));
        validationError.Should().BeNull();
    }

    [TestMethod]
    public async Task OrganisationMainActivitySicValidation_WhenMainActivitySicInvalid_ThenError()
    {
        // Arrange
        var dataRow = RowDataTestHelper.GenerateOrgs(1).First();
        var service = CreateService();

        dataRow.MainActivitySic = "123456";
        var blobQueueMessage = new BlobQueueMessage();

        // Act
        var errors = await service.ValidateOrganisationsAsync(new List<OrganisationDataRow>() { dataRow }, blobQueueMessage, false);

        // Assert
        var columnError = errors.Single().ColumnErrors.Single();

        columnError.ColumnName.Should().Be("main_activity_sic");
        columnError.ErrorCode.Should().Be(ErrorCodes.MainActivitySicNotFiveDigitsInteger);
    }

    [TestMethod]
    public async Task OrganisationTradingNameValidation_WhenTradingNameSameAsOrganisationName_ThenError()
    {
        // Arrange
        var dataRow = RowDataTestHelper.GenerateOrgs(1).First();
        var service = CreateService();

        dataRow.TradingName = dataRow.OrganisationName;
        var blobQueueMessage = new BlobQueueMessage();

        // Act
        var errors = await service.ValidateOrganisationsAsync(new List<OrganisationDataRow>() { dataRow }, blobQueueMessage, false);

        // Assert
        var columnError = errors.Single().ColumnErrors.Single();

        columnError.ColumnName.Should().Be("trading_name");
        columnError.ErrorCode.Should().Be(ErrorCodes.TradingNameSameAsOrganisationName);
    }

    [TestMethod]
    public async Task IsColumnLengthExceeded_WhenRowDoesNotExceedCharacterLimit_ReturnsFalse()
    {
        // Arrange
        const int rowCount = 10;
        var dataRows = RowDataTestHelper.GenerateOrganisationCSVFile(rowCount);
        var service = CreateService();

        // Act
        var results = service.IsColumnLengthExceeded(dataRows.ToList());

        // Assert
        results.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public async Task IsColumnLengthExceeded_WhenRowExceedsCharacterLimit_ReturnsTrue(int index)
    {
        // Arrange
        var service = CreateService();
        var dataRows = RowDataTestHelper.GenerateOrganisationWithExceededCharacterLimit(index);

        // Act
        var results = service.IsColumnLengthExceeded(dataRows);

        // Assert
        results.Should().BeTrue();
    }

    [TestMethod]
    public async Task Validate_BrandFile_ExpectNoValidationError()
    {
        // Arrange
        const int rowCount = 10;
        var dataRows = RowDataTestHelper.GenerateBrand(rowCount).ToList();
        var service = CreateService();

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows.ToList(), null);

        // Assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_BrandFile_With_NonMatching_OrganisationId_ExpectValidationError()
    {
        // Arrange
        const int rowCount = 1;
        var dataRows = RowDataTestHelper.GenerateBrand(rowCount).ToList();
        var service = CreateService();

        var organisationDataTable = OrganisationDataLookupTableTestHelper
            .GenerateWithValues(dataRows[0].DefraId + "99", dataRows[0].SubsidiaryId);

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows.ToList(), organisationDataTable);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCount(1);
        results[0].Should().Be(ErrorCodes.BrandDetailsNotMatchingOrganisation);
    }

    [TestMethod]
    public async Task Validate_BrandFile_With_NonMatching_SubsidiaryId_ExpectValidationError()
    {
        // Arrange
        const int rowCount = 1;
        var dataRows = RowDataTestHelper.GenerateBrand(rowCount).ToList();
        var service = CreateService();

        var organisationDataTable = OrganisationDataLookupTableTestHelper
            .GenerateWithValues(dataRows[0].DefraId, dataRows[0].SubsidiaryId + "99");

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows.ToList(), organisationDataTable);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCount(1);
        results[0].Should().Be(ErrorCodes.BrandDetailsNotMatchingSubsidiary);
    }

    [TestMethod]
    public async Task Validate_BrandFile_With_Empty_LookupTable_ExpectNoValidationError()
    {
        // Arrange
        const int rowCount = 10;
        var dataRows = RowDataTestHelper.GenerateBrand(rowCount).ToList();
        var service = CreateService();

        var organisationDataTable = OrganisationDataLookupTableTestHelper.GenerateEmptyTable();

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows.ToList(), organisationDataTable);

        // Assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_BrandFile_With_Matching_OrganisationData_ExpectNoValidationError()
    {
        // Arrange
        const int rowCount = 5;
        var dataRows = RowDataTestHelper.GenerateBrand(rowCount).ToList();
        var service = CreateService();

        var organisationDataTable = OrganisationDataLookupTableTestHelper.GenerateFromCsvRows(dataRows);

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows.ToList(), organisationDataTable);

        // Assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_PartnerFile_With_NonMatching_OrganisationId_ExpectValidationError()
    {
        // Arrange
        const int rowCount = 1;
        var dataRows = RowDataTestHelper.GeneratePartner(rowCount).ToList();
        var service = CreateService();

        var organisationDataTable = OrganisationDataLookupTableTestHelper
            .GenerateWithValues(dataRows[0].DefraId + "99", dataRows[0].SubsidiaryId);

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows.ToList(), organisationDataTable);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCount(1);
        results[0].Should().Be(ErrorCodes.PartnerDetailsNotMatchingOrganisation);
    }

    [TestMethod]
    public async Task Validate_PartnerFile_With_NonMatching_SubsidiaryId_ExpectValidationError()
    {
        // Arrange
        const int rowCount = 1;
        var dataRows = RowDataTestHelper.GeneratePartner(rowCount).ToList();
        var service = CreateService();

        var organisationDataTable = OrganisationDataLookupTableTestHelper
            .GenerateWithValues(dataRows[0].DefraId, dataRows[0].SubsidiaryId + "99");

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows.ToList(), organisationDataTable);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCount(1);
        results[0].Should().Be(ErrorCodes.PartnerDetailsNotMatchingOrganisation);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public async Task Validate_BrandFile_AndCharacterLimitExceeded_ExpectValidationError(int columnIndex)
    {
        // Arrange
        var service = CreateService();
        var dataRows = RowDataTestHelper.GenerateBrandWithExceededCharacterLimit(columnIndex);

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows, null);

        // Assert
        results.Should().NotBeNull();
        results[0].Should().Be(ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public async Task Validate_PartnerFile_ExpectNoValidationError()
    {
        // Arrange
        const int rowCount = 10;
        var dataRows = RowDataTestHelper.GeneratePartner(rowCount).ToList();
        var service = CreateService();

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows.ToList(), null);

        // Assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_PartnerFile_With_Empty_LookupTable_ExpectNoValidationError()
    {
        // Arrange
        const int rowCount = 10;
        var dataRows = RowDataTestHelper.GeneratePartner(rowCount).ToList();
        var service = CreateService();

        var organisationDataTable = OrganisationDataLookupTableTestHelper.GenerateEmptyTable();

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows.ToList(), organisationDataTable);

        // Assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_PartnerFile_With_Matching_OrganisationData_ExpectNoValidationError()
    {
        // Arrange
        const int rowCount = 5;
        var dataRows = RowDataTestHelper.GeneratePartner(rowCount).ToList();
        var service = CreateService();

        dataRows.Add(new PartnersDataRow { DefraId = "199", SubsidiaryId = null });
        dataRows.Add(new PartnersDataRow { DefraId = "101", SubsidiaryId = null });
        dataRows.Add(new PartnersDataRow { DefraId = "101", SubsidiaryId = "101" });
        dataRows.Add(new PartnersDataRow { DefraId = "101", SubsidiaryId = "102" });
        dataRows.Add(new PartnersDataRow { DefraId = "101", SubsidiaryId = "102" });
        var organisationDataTable = OrganisationDataLookupTableTestHelper.GenerateFromCsvRows(dataRows);

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows.ToList(), organisationDataTable);

        // Assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(5)]
    [DataRow(6)]
    public async Task Validate_PartnerFile_AndCharacterLimitExceeded_ExpectValidationError(int index)
    {
        // Arrange
        var service = CreateService();
        var dataRows = RowDataTestHelper.GeneratePartnerWithExceededCharacterLimit(index);

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows, null);

        // Assert
        results.Should().NotBeNull();
        results[0].Should().Be(ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public void ValidateAppendedFileAsync_WithInvalidRowType_ThrowsException()
    {
        // Arrange
        var service = CreateService();
        var invalidRowTypes = new[] { new InvalidRowType() };

        // Act
        var act = async () => await service.ValidateAppendedFileAsync(invalidRowTypes.ToList(), null);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    public async Task ValidateAppendedFileAsync_WithSameErrorInMultipleRows_OnlyAddsErrorOnce()
    {
        // Arrange
        const int rowCount = 2;
        var dataRows = RowDataTestHelper.GeneratePartner(rowCount).ToList();
        dataRows[0].DefraId = new string('a', CharacterLimits.MaxLength + 1);
        dataRows[1].DefraId = new string('b', CharacterLimits.MaxLength + 1);
        var service = CreateService();

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows, null);

        // Assert
        results.Should().NotBeNull();
        results[0].Should().Be(ErrorCodes.CharacterLengthExceeded);
        results.Count.Should().Be(1);
    }

    [TestMethod]
    public async Task ValidateOrganisationsAsync_AsProducerUser_With_ValidateCompanyDetails_False()
    {
        // Arrange
        const int rowCount = 4;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var organisation = new CompanyDetailsDataItem
        {
            ReferenceNumber = "123456",
            CompaniesHouseNumber = "X1234567",
        };

        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(organisation);
        var companyDetailsDataResult = new CompanyDetailsDataResult();
        companyDetailsDataResult.Organisations = companyDetailsOrganisations;

        _companyDetailsApiClientMock
            .Setup(f => f.GetCompanyDetailsByProducer(It.IsAny<string>()))
            .ReturnsAsync(companyDetailsDataResult);

        var blobQueueMessage = new BlobQueueMessage();

        // Act
        var errors = await service.ValidateOrganisationsAsync(dataRows.ToList(), blobQueueMessage, false);

        // Assert
        errors.Should().BeEmpty();

        _companyDetailsApiClientMock.Verify(
            m => m.GetCompanyDetailsByProducer(It.IsAny<string>()),
            Times.Never());
    }

    [TestMethod]
    public async Task ValidateOrganisationsAsync_AsProducerUser_With_ValidateCompanyDetails_True_FailureErrorMessage()
    {
        // Arrange
        const int rowCount = 6;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var organisation = new CompanyDetailsDataItem
        {
            ReferenceNumber = "123456",
            CompaniesHouseNumber = "X1234567",
        };

        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(organisation);
        var companyDetailsDataResult = new CompanyDetailsDataResult();
        companyDetailsDataResult.Organisations = companyDetailsOrganisations;

        _companyDetailsApiClientMock
            .Setup(f => f.GetCompanyDetailsByProducer(It.IsAny<string>()))
            .ReturnsAsync(companyDetailsDataResult);

        var blobQueueMessage = new BlobQueueMessage();

        // Act
        var errors = await service.ValidateOrganisationsAsync(dataRows.ToList(), blobQueueMessage, true);

        // Assert
        var validationError = errors.Find(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.CheckOrganisationId));
        validationError.Should().NotBeNull();

        _companyDetailsApiClientMock.Verify(
            m => m.GetCompanyDetailsByProducer(It.IsAny<string>()),
            Times.AtLeastOnce());
    }

    [TestMethod]
    public async Task ValidateCompanyDetails_AsProducerUser()
    {
        // Arrange
        const int rowCount = 4;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].DefraId = "100001";
        dataRows[0].CompaniesHouseNumber = "110011";
        dataRows[1].DefraId = "200002";
        dataRows[1].CompaniesHouseNumber = "220022";
        dataRows[2].DefraId = "300003";
        dataRows[2].CompaniesHouseNumber = "330033";
        dataRows[3].DefraId = "400004";
        dataRows[3].CompaniesHouseNumber = "440044";
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(new CompanyDetailsDataItem { ReferenceNumber = "100001", CompaniesHouseNumber = "110011" });
        companyDetailsOrganisations.Add(new CompanyDetailsDataItem { ReferenceNumber = "200002", CompaniesHouseNumber = "220022" });
        companyDetailsOrganisations.Add(new CompanyDetailsDataItem { ReferenceNumber = "300003", CompaniesHouseNumber = "330033" });
        companyDetailsOrganisations.Add(new CompanyDetailsDataItem { ReferenceNumber = "400004", CompaniesHouseNumber = "440044" });
        var companyDetailsDataResult1 = new CompanyDetailsDataResult { Organisations = companyDetailsOrganisations };

        _companyDetailsApiClientMock
            .Setup(f => f.GetCompanyDetailsByProducer("3E40CA85-D788-4E3D-A2E1-18C18FAEA6DF"))
            .ReturnsAsync(companyDetailsDataResult1);

        // Act
        var results = await service.ValidateCompanyDetails(new ValidateCompanyDetailsModel
        {
            OrganisationDataRows = dataRows.ToList(),
            TotalErrors = 0,
            ComplianceSchemeId = string.Empty,
            UserId = string.Empty,
            ProducerOrganisationId = "3E40CA85-D788-4E3D-A2E1-18C18FAEA6DF",
        });

        // Assert
        results.ValidationErrors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateCompanyDetails_AsComplianceSchemeUser()
    {
        // Arrange
        const int rowCount = 4;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].DefraId = "100001";
        dataRows[0].CompaniesHouseNumber = "110011";
        dataRows[1].DefraId = "200002";
        dataRows[1].CompaniesHouseNumber = "220022";
        dataRows[2].DefraId = "300003";
        dataRows[2].CompaniesHouseNumber = "330033";
        dataRows[3].DefraId = "400004";
        dataRows[3].CompaniesHouseNumber = "440044";
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var companyDetailsOrganisations1 = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations1.Add(new CompanyDetailsDataItem { ReferenceNumber = "100001", CompaniesHouseNumber = "110011" });
        var complianceSchemeMembers = new CompanyDetailsDataResult { Organisations = companyDetailsOrganisations1 };

        var producerOrganisations = new List<CompanyDetailsDataItem>();
        producerOrganisations.Add(new CompanyDetailsDataItem { ReferenceNumber = "200002", CompaniesHouseNumber = "220022" });
        producerOrganisations.Add(new CompanyDetailsDataItem { ReferenceNumber = "300003", CompaniesHouseNumber = "330033" });
        producerOrganisations.Add(new CompanyDetailsDataItem { ReferenceNumber = "400004", CompaniesHouseNumber = "440044" });
        var remainingProducers = new CompanyDetailsDataResult { Organisations = producerOrganisations };

        _companyDetailsApiClientMock
            .Setup(f => f.GetComplianceSchemeMembers("100001", "85a8b24f-d192-461f-8a0b-87dc54f63453"))
            .ReturnsAsync(complianceSchemeMembers);
        _companyDetailsApiClientMock
            .Setup(f => f.GetRemainingProducerDetails(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(remainingProducers);

        // Act
        var results = await service.ValidateCompanyDetails(new ValidateCompanyDetailsModel
        {
            OrganisationDataRows = dataRows.ToList(),
            TotalErrors = 0,
            ComplianceSchemeId = "85a8b24f-d192-461f-8a0b-87dc54f63453",
            UserId = string.Empty,
            ProducerOrganisationId = string.Empty,
        });

        // Assert
        results.ValidationErrors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateCompanyDetails_AsProducerUser_FailureErrorMessage()
    {
        // Arrange
        const int rowCount = 6;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var organisation = new CompanyDetailsDataItem
        {
            ReferenceNumber = "123456",
            CompaniesHouseNumber = "X1234567",
        };

        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(organisation);
        var companyDetailsDataResult = new CompanyDetailsDataResult();
        companyDetailsDataResult.Organisations = companyDetailsOrganisations;

        _companyDetailsApiClientMock
            .Setup(f => f.GetCompanyDetailsByProducer(It.IsAny<string>()))
            .ReturnsAsync(companyDetailsDataResult);

        // Act
        var results = await service.ValidateCompanyDetails(new ValidateCompanyDetailsModel
        {
            OrganisationDataRows = dataRows.ToList(),
            TotalErrors = 0,
            ComplianceSchemeId = string.Empty,
            UserId = string.Empty,
            ProducerOrganisationId = string.Empty,
        });

        // Assert
        results.ValidationErrors.Should().Match(x => x.Any(x => x.ColumnErrors.Any(e =>
            e.ErrorCode == ErrorCodes.CheckOrganisationId &&
            e.ColumnName == "organisation_id")));
    }

    [TestMethod]
    public async Task ValidateCompanyDetails_AsProducerUser_With_Invalid_CompaniesHouseNumber_FailureErrorMessage()
    {
        // Arrange
        const int rowCount = 1;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].CompaniesHouseNumber = "99999999";

        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var organisation = new CompanyDetailsDataItem
        {
            ReferenceNumber = dataRows[0].DefraId,
            CompaniesHouseNumber = "X1234567",
        };

        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(organisation);
        var companyDetailsDataResult = new CompanyDetailsDataResult();
        companyDetailsDataResult.Organisations = companyDetailsOrganisations;

        _companyDetailsApiClientMock
            .Setup(f => f.GetCompanyDetailsByProducer(It.IsAny<string>()))
            .ReturnsAsync(companyDetailsDataResult);

        // Act
        var results = await service.ValidateCompanyDetails(new ValidateCompanyDetailsModel
        {
            OrganisationDataRows = dataRows.ToList(),
            TotalErrors = 0,
            ComplianceSchemeId = string.Empty,
            UserId = string.Empty,
            ProducerOrganisationId = string.Empty,
        });

        // Assert
        results.ValidationErrors.Should().Match(x => x.Any(x => x.ColumnErrors.Any(e =>
            e.ErrorCode == ErrorCodes.CompaniesHouseNumberNotMatchOrganisationId &&
            e.ColumnName == "organisation_id")));
        results.ValidationErrors.Should().Match(x => x.Any(x => x.ColumnErrors.Any(e =>
            e.ErrorCode == ErrorCodes.CompaniesHouseNumberNotMatchOrganisationId &&
            e.ColumnName == "companies_house_number")));
    }

    [TestMethod]
    public async Task ValidateCompanyDetails_AsProducerUser_Logs_Http_Failure()
    {
        // Arrange
        const int rowCount = 6;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        _companyDetailsApiClientMock
            .Setup(f => f.GetCompanyDetailsByProducer(It.IsAny<string>()))
            .Throws<HttpRequestException>();

        // Act
        var results = await service.ValidateCompanyDetails(new ValidateCompanyDetailsModel
        {
            OrganisationDataRows = dataRows.ToList(),
            TotalErrors = 0,
            ComplianceSchemeId = string.Empty,
            UserId = string.Empty,
            ProducerOrganisationId = string.Empty,
        });

        // Assert
        results.ValidationErrors.Should().BeEmpty();

        _loggerMock.VerifyLog(logger => logger.LogError(It.IsAny<HttpRequestException>(), It.IsAny<string>()));
    }

    [TestMethod]
    public async Task ValidateCompanyDetails_AsComplianceSchemeUser_FailureErrorMessage()
    {
        // Arrange
        const int rowCount = 6;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var organisation = new CompanyDetailsDataItem
        {
            ReferenceNumber = "123456",
            CompaniesHouseNumber = "X1234567",
        };

        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(organisation);
        var companyDetailsDataResult = new CompanyDetailsDataResult();
        companyDetailsDataResult.Organisations = companyDetailsOrganisations;

        _companyDetailsApiClientMock
            .Setup(f => f.GetComplianceSchemeMembers(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(companyDetailsDataResult);
        _companyDetailsApiClientMock
            .Setup(f => f.GetRemainingProducerDetails(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(companyDetailsDataResult);

        // Act
        var results = await service.ValidateCompanyDetails(new ValidateCompanyDetailsModel
        {
            OrganisationDataRows = dataRows.ToList(),
            TotalErrors = 0,
            ComplianceSchemeId = "13202f0d-bde8-422c-974a-f1dec1b32fff",
            UserId = string.Empty,
            ProducerOrganisationId = string.Empty,
        });

        // Assert
        _companyDetailsApiClientMock.Verify(
            m => m.GetComplianceSchemeMembers(It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(rowCount));
        _companyDetailsApiClientMock.Verify(
            m => m.GetRemainingProducerDetails(It.IsAny<IEnumerable<string>>()),
            Times.Once);

        results.TotalErrors.Should().Be(6);
        results.ValidationErrors.Should().Match(x => x.Any(x => x.ColumnErrors.Any(e =>
            e.ErrorCode == ErrorCodes.CheckOrganisationId &&
            e.ColumnName == "organisation_id")));
    }

    [TestMethod]
    public async Task ValidateCompanyDetails_AsComplianceSchemeUser_With_Invalid_CompaniesHouseNumber_FailureErrorMessage()
    {
        // Arrange
        const int rowCount = 1;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].CompaniesHouseNumber = "99999999";

        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var organisation = new CompanyDetailsDataItem
        {
            ReferenceNumber = dataRows[0].DefraId,
            CompaniesHouseNumber = "X1234567",
        };

        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(organisation);
        var complianceSchemeMembers = new CompanyDetailsDataResult();
        complianceSchemeMembers.Organisations = companyDetailsOrganisations;

        _companyDetailsApiClientMock
            .Setup(f => f.GetComplianceSchemeMembers(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(complianceSchemeMembers);

        // Act
        var results = await service.ValidateCompanyDetails(new ValidateCompanyDetailsModel
        {
            OrganisationDataRows = dataRows.ToList(),
            TotalErrors = 0,
            ComplianceSchemeId = "13202f0d-bde8-422c-974a-f1dec1b32fff",
            UserId = string.Empty,
            ProducerOrganisationId = string.Empty,
        });

        // Assert
        _companyDetailsApiClientMock.Verify(
            m => m.GetComplianceSchemeMembers(It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(rowCount));
        _companyDetailsApiClientMock.Verify(
            m => m.GetRemainingProducerDetails(It.IsAny<IEnumerable<string>>()),
            Times.Never);

        results.ValidationErrors.Should().Match(x => x.Any(x => x.ColumnErrors.Any(e =>
            e.ErrorCode == ErrorCodes.CompaniesHouseNumberNotMatchOrganisationId &&
            e.ColumnName == "organisation_id")));
        results.ValidationErrors.Should().Match(x => x.Any(x => x.ColumnErrors.Any(e =>
            e.ErrorCode == ErrorCodes.CompaniesHouseNumberNotMatchOrganisationId &&
            e.ColumnName == "companies_house_number")));
    }

    [TestMethod]
    public async Task ValidateCompanyDetails_AsComplianceSchemeUser_AdditionalMembers_FailureErrorMessage()
    {
        // Arrange
        const int rowCount = 6;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var organisation = new CompanyDetailsDataItem
        {
            ReferenceNumber = "123456",
            CompaniesHouseNumber = "X1234567",
        };

        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(organisation);
        var complianceSchemeMembers = new CompanyDetailsDataResult();
        complianceSchemeMembers.Organisations = companyDetailsOrganisations;

        _companyDetailsApiClientMock
            .Setup(f => f.GetComplianceSchemeMembers(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(complianceSchemeMembers);
        _companyDetailsApiClientMock
            .Setup(f => f.GetRemainingProducerDetails(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(default(CompanyDetailsDataResult));

        // Act
        var results = await service.ValidateCompanyDetails(new ValidateCompanyDetailsModel
        {
            OrganisationDataRows = dataRows.ToList(),
            TotalErrors = 0,
            ComplianceSchemeId = "13202f0d-bde8-422c-974a-f1dec1b32fff",
            UserId = string.Empty,
            ProducerOrganisationId = string.Empty,
        });

        // Assert
        _companyDetailsApiClientMock.Verify(
            m => m.GetComplianceSchemeMembers(It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(rowCount));
        _companyDetailsApiClientMock.Verify(
           m => m.GetRemainingProducerDetails(It.IsAny<IEnumerable<string>>()),
           Times.Once);

        results.ValidationErrors.Should().Match(x => x.Any(x => x.ColumnErrors.Any(e =>
            e.ErrorCode == ErrorCodes.CheckOrganisationId &&
            e.ColumnName == "organisation_id")));
    }

    [TestMethod]
    public async Task ValidateCompanyDetails_AsComplianceSchemeUser_AdditionalMembers_With_Invalid_CompaniesHouseNumber_FailureErrorMessage()
    {
        // Arrange
        const int rowCount = 2;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].CompaniesHouseNumber = "99999999";

        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var organisation = new CompanyDetailsDataItem
        {
            ReferenceNumber = dataRows[0].DefraId,
            CompaniesHouseNumber = "X1234567",
        };

        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(organisation);
        var companyDetailsDataResult = new CompanyDetailsDataResult();
        companyDetailsDataResult.Organisations = companyDetailsOrganisations;

        _companyDetailsApiClientMock
            .Setup(f => f.GetComplianceSchemeMembers(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(companyDetailsDataResult);
        _companyDetailsApiClientMock
            .Setup(f => f.GetRemainingProducerDetails(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(companyDetailsDataResult);

        // Act
        var results = await service.ValidateCompanyDetails(new ValidateCompanyDetailsModel
        {
            OrganisationDataRows = dataRows.ToList(),
            TotalErrors = 0,
            ComplianceSchemeId = "13202f0d-bde8-422c-974a-f1dec1b32fff",
            UserId = string.Empty,
            ProducerOrganisationId = string.Empty,
        });

        // Assert
        _companyDetailsApiClientMock.Verify(
            m => m.GetComplianceSchemeMembers(It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(rowCount));
        _companyDetailsApiClientMock.Verify(
            m => m.GetRemainingProducerDetails(It.IsAny<IEnumerable<string>>()),
            Times.Once);

        results.ValidationErrors.Should().Match(x => x.Any(x => x.ColumnErrors.Any(e =>
            e.ErrorCode == ErrorCodes.CompaniesHouseNumberNotMatchOrganisationId &&
            e.ColumnName == "organisation_id")));
        results.ValidationErrors.Should().Match(x => x.Any(x => x.ColumnErrors.Any(e =>
            e.ErrorCode == ErrorCodes.CompaniesHouseNumberNotMatchOrganisationId &&
            e.ColumnName == "companies_house_number")));
    }

    [TestMethod]
    public async Task ValidateCompanyDetails_AsComplianceSchemeUser_AdditionalMembersWithProducers_FailureErrorMessage()
    {
        // Arrange
        int rowCount = 4;
        int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var organisation = new CompanyDetailsDataItem
        {
            ReferenceNumber = "123456",
            CompaniesHouseNumber = "X1234567",
        };

        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(organisation);
        var companyDetailsDataResult = new CompanyDetailsDataResult();
        companyDetailsDataResult.Organisations = companyDetailsOrganisations;
        var remainingProducers = new CompanyDetailsDataResult();
        var remainingProducerOrganisations = new List<CompanyDetailsDataItem>();
        remainingProducerOrganisations.Add(new CompanyDetailsDataItem { CompaniesHouseNumber = "123", ReferenceNumber = dataRows[0].DefraId });
        remainingProducerOrganisations.Add(new CompanyDetailsDataItem { CompaniesHouseNumber = "456", ReferenceNumber = dataRows[1].DefraId });
        remainingProducerOrganisations.Add(new CompanyDetailsDataItem { CompaniesHouseNumber = "789", ReferenceNumber = dataRows[2].DefraId });
        remainingProducerOrganisations.Add(new CompanyDetailsDataItem { CompaniesHouseNumber = "111", ReferenceNumber = dataRows[3].DefraId });
        remainingProducers.Organisations = remainingProducerOrganisations;

        _companyDetailsApiClientMock
            .Setup(f => f.GetComplianceSchemeMembers(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(companyDetailsDataResult);

        _companyDetailsApiClientMock
            .Setup(f => f.GetRemainingProducerDetails(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(remainingProducers);

        // Act
        var results = await service.ValidateCompanyDetails(new ValidateCompanyDetailsModel
        {
            OrganisationDataRows = dataRows.ToList(),
            TotalErrors = 0,
            ComplianceSchemeId = "13202f0d-bde8-422c-974a-f1dec1b32fff",
            UserId = string.Empty,
            ProducerOrganisationId = string.Empty,
        });

        // Assert
        var validationError = results.ValidationErrors.Find(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.CheckOrganisationId));
        validationError.Should().BeNull();

        _companyDetailsApiClientMock.Verify(
            m => m.GetComplianceSchemeMembers(It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(rowCount));
        _companyDetailsApiClientMock.Verify(
            m => m.GetRemainingProducerDetails(It.IsAny<IEnumerable<string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ValidateCompanyDetails_AsProducerUser_HasValidOrganisationId()
    {
        // Arrange
        const int rowCount = 2;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].DefraId = "100001";
        dataRows[0].CompaniesHouseNumber = "110011";
        dataRows[1].DefraId = "200002";
        dataRows[1].CompaniesHouseNumber = "220022";
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(new CompanyDetailsDataItem { ReferenceNumber = "100001", CompaniesHouseNumber = "110011" });
        companyDetailsOrganisations.Add(new CompanyDetailsDataItem { ReferenceNumber = "200002", CompaniesHouseNumber = "220022" });
        var companyDetailsDataResult = new CompanyDetailsDataResult { Organisations = companyDetailsOrganisations };

        _companyDetailsApiClientMock
            .Setup(f => f.GetCompanyDetailsByProducer("3E40CA85-D788-4E3D-A2E1-18C18FAEA6DF"))
            .ReturnsAsync(companyDetailsDataResult);

        // Act
        var results = await service.ValidateCompanyDetails(new ValidateCompanyDetailsModel
        {
            OrganisationDataRows = dataRows.ToList(),
            TotalErrors = 0,
            ComplianceSchemeId = string.Empty,
            UserId = string.Empty,
            ProducerOrganisationId = "3E40CA85-D788-4E3D-A2E1-18C18FAEA6DF",
        });

        // Assert
        results.ValidationErrors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateCompanyDetails_AsProducerUser_HasInvalidOrganisationId_FailureErrorMessage()
    {
        // Arrange
        const int rowCount = 2;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].DefraId = "100001";
        dataRows[0].CompaniesHouseNumber = "110011";
        dataRows[1].DefraId = "200002";
        dataRows[1].CompaniesHouseNumber = "220022";
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var companyDetailsOrganisations1 = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations1.Add(new CompanyDetailsDataItem { ReferenceNumber = "100001", CompaniesHouseNumber = "110011" });
        var companyDetailsDataResult1 = new CompanyDetailsDataResult { Organisations = companyDetailsOrganisations1 };
        var companyDetailsOrganisations2 = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations2.Add(new CompanyDetailsDataItem { ReferenceNumber = "200002", CompaniesHouseNumber = "220022" });
        var companyDetailsDataResult2 = new CompanyDetailsDataResult { Organisations = companyDetailsOrganisations2 };

        _companyDetailsApiClientMock
            .Setup(f => f.GetCompanyDetailsByProducer("3E40CA85-D788-4E3D-A2E1-18C18FAEA6DF"))
            .ReturnsAsync(companyDetailsDataResult1);
        _companyDetailsApiClientMock
            .Setup(f => f.GetCompanyDetailsByProducer("3E40CA85-D788-4E3D-A2E1-18C18FAEA6DF"))
            .ReturnsAsync(companyDetailsDataResult2);

        // Act
        var results = await service.ValidateCompanyDetails(new ValidateCompanyDetailsModel
        {
            OrganisationDataRows = dataRows.ToList(),
            TotalErrors = 0,
            ComplianceSchemeId = string.Empty,
            UserId = string.Empty,
            ProducerOrganisationId = "2CF8A130-2568-4C7E-AAA0-3CB341EF9C76",
        });

        // Assert
        results.TotalErrors.Should().Be(2);
        results.ValidationErrors.Should().Match(x => x.Any(x => x.ColumnErrors.Any(e =>
            e.ErrorCode == ErrorCodes.CheckOrganisationId &&
            e.ColumnName == "organisation_id")));
    }

    private ValidationService CreateService(ValidationSettings? settings = null)
    {
        _companyDetailsApiClientMock = new Mock<ICompanyDetailsApiClient>();

        return new ValidationService(
            new OrganisationDataRowValidator(),
            new BrandDataRowValidator(),
            new PartnerDataRowValidator(),
            new ColumnMetaDataProvider(),
            Options.Create(settings ?? new ValidationSettings()),
            _companyDetailsApiClientMock.Object,
            _loggerMock.Object);
    }

    private class InvalidRowType : ICsvDataRow
    {
        public int LineNumber { get; set; }
    }
}