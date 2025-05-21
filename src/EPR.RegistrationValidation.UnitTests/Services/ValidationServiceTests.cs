namespace EPR.RegistrationValidation.UnitTests.Services;

using EPR.RegistrationValidation.Application.Clients;
using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Helpers;
using EPR.RegistrationValidation.Application.Services;
using EPR.RegistrationValidation.Application.Services.Subsidiary;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Config;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.Data.Models.CompanyDetailsApi;
using EPR.RegistrationValidation.Data.Models.QueueMessages;
using EPR.RegistrationValidation.Data.Models.Services;
using EPR.RegistrationValidation.Data.Models.SubmissionApi;
using EPR.RegistrationValidation.Data.Models.Subsidiary;
using EPR.RegistrationValidation.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class ValidationServiceTests
{
    private Mock<ICompanyDetailsApiClient> _companyDetailsApiClientMock;
    private Mock<ILogger<ValidationService>> _loggerMock;
    private Mock<ISubsidiaryDetailsRequestBuilder> _subsidiaryDetailsRequestBuilderMock;
    private Mock<IFeatureManager> _featureManagerMock;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<ValidationService>>();
        _subsidiaryDetailsRequestBuilderMock = new Mock<ISubsidiaryDetailsRequestBuilder>();
        _featureManagerMock = new Mock<IFeatureManager>();
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
    public async Task Validate_WithSubFeatureFlagOn_ShouldCallSubValidation()
    {
        // Arrange
        const int expectedRow = 0;
        const int expectedColumnIndex = 0;
        const string expectedColumnName = "organisation_id";
        var service = CreateService();
        var blobQueueMessage = new BlobQueueMessage();
        var dataRows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1", LineNumber = 1 },
        };
        _featureManagerMock
            .Setup(fm => fm.IsEnabledAsync(FeatureFlags.EnableSubsidiaryValidation))
            .ReturnsAsync(true);
        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false },
                    },
                },
            },
        };

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(subsidiaryDetailsResponse);

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows, blobQueueMessage, false);

        // Assert
        var validationError = results.Find(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.SubsidiaryIdBelongsToDifferentOrganisation));
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
                OrganisationSize = OrganisationSizeCodes.L.ToString(),
                TotalTonnage = "50.1",
                RegistrationTypeCode = RegistrationTypeCodes.Group,
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
        var results = await service.ValidateRowsAsync(dataRows.ToList(), false);

        // Assert
        results.TotalErrors.Should().Be(maxErrors);
    }

    [TestMethod]
    public async Task ValidateOrganisationWarningsAsync_Validates_Return_NoWarnings()
    {
        // Arrange
        const int rowCount = 20;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateInvalidOrgs(rowCount);

        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = await service.ValidateOrganisationWarningsAsync(dataRows.ToList());

        // Assert
        results.Count.Should().Be(0);
    }

    [TestMethod]
    public async Task ValidateOrganisationWarningsAsync_Validates_With_Warning()
    {
        // Arrange
        var i = 123;
        const int maxErrors = 1;

        List<OrganisationDataRow> orgDataRowList = new()
        {
            new OrganisationDataRow
            {
                DefraId = "12345" + i,
                SubsidiaryId = "678",
                OrganisationName = $"{i} ltd",
                HomeNationCode = "EN",
                PrimaryContactPersonFirstName = $"{i}FName",
                PrimaryContactPersonLastName = $"{i}LName",
                PrimaryContactPersonEmail = $"email{i}@test.com",
                PrimaryContactPersonPhoneNumber = $"07895462{i}",
                PackagingActivitySO = "No",
                PackagingActivityHl = "No",
                PackagingActivityPf = "No",
                PackagingActivitySl = "No",
                PackagingActivityIm = "No",
                PackagingActivityOm = "No",
                PackagingActivitySe = "No",
                ProduceBlankPackagingFlag = "No",
                LiableForDisposalCostsFlag = "Yes",
                MeetReportingRequirementsFlag = "Yes",
                Turnover = "0",
                ServiceOfNoticeAddressLine1 = "9 Surrey",
                ServiceOfNoticeAddressPostcode = "KT5 8JU",
                ServiceOfNoticeAddressPhoneNumber = "0123456789",
                AuditAddressLine1 = "10 Southcote",
                AuditAddressCountry = AuditingCountryCodes.England.ToLower(),
                AuditAddressPostcode = "KT5 9UW",
                TotalTonnage = "25",
                PrincipalAddressLine1 = "Principal Address Line 1",
                PrincipalAddressPostcode = "Principal Address Postcode",
                PrincipalAddressPhoneNumber = "01237946",
                OrganisationTypeCode = UnIncorporationTypeCodes.SoleTrader,
                OrganisationSize = OrganisationSizeCodes.L.ToString(),
                JoinerDate = "01/01/2000",
                RegistrationTypeCode = RegistrationTypeCodes.Individual,
            },
        };

        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = await service.ValidateOrganisationWarningsAsync(orgDataRowList);

        // Assert
        results.Count.Should().Be(1);
        results[0].ColumnErrors
            .Should()
            .HaveCount(1);

        results[0].ColumnErrors.Select(x => x.ErrorCode)
            .Should()
            .HaveCount(1)
            .And.BeEquivalentTo("73");
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
    public async Task ValidateOrganisationSubsidiaryRelationships_WithMissingOrgParent_ReturnsLimitedErrors()
    {
        // Arrange
        const int rowCount = 20;
        const int maxErrors = 25;
        const int initialTotalErrors = 5;
        const int expectedErrors = 20;
        var dataRows = RowDataTestHelper.GenerateOrgIdSubIdWithoutParentOrg(rowCount);

        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = service.ValidateOrganisationSubsidiaryRelationships(dataRows.ToList(), initialTotalErrors);

        // Assert
        results.TotalErrors.Should().Be(maxErrors);
        results.ValidationErrors.SelectMany(x => x.ColumnErrors)
            .Count(x => x.ErrorCode == ErrorCodes.MissingOrganisationId).Should()
            .Be(expectedErrors);
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
        results[0].Should().Be(ErrorCodes.PartnerDetailsNotMatchingSubsidiary);
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
        dataRows[0].OrganisationTypeCode = "LTD";

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
    [DataRow("OUT")]
    [DataRow("COP")]
    [DataRow("SOL")]
    [DataRow("PAR")]
    [DataRow("OTH")]
    public async Task ValidateCompanyDetails_AsProducerUser_With_No_CompaniesHouseNumber(string organisationTypeCode)
    {
        // Arrange
        const int rowCount = 1;
        const int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].CompaniesHouseNumber = string.Empty;
        dataRows[0].OrganisationTypeCode = organisationTypeCode;

        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        var organisation = new CompanyDetailsDataItem
        {
            ReferenceNumber = dataRows[0].DefraId,
            CompaniesHouseNumber = string.Empty,
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
        results.ValidationErrors.Should().BeEmpty();
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
        dataRows[0].OrganisationTypeCode = "LTD";

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
        dataRows[0].OrganisationTypeCode = "LTD";

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

    [TestMethod]
    public async Task ValidateSubsidiary_ShouldReturnTotalErrorsAndValidationErrors_WhenNoErrorsFound()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1", LineNumber = 1 },
        };

        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = true },
                    },
                },
            },
        };
        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false },
                    },
                },
            },
        };
        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(subsidiaryDetailsRequest);

        var service = CreateService(new ValidationSettings { ErrorLimit = 50 });

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(subsidiaryDetailsResponse);

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(0, totalErrors);
        Assert.AreEqual(0, validationErrors.Count);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_ShouldAddError_WhenSubsidiaryDoesNotExist()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1", LineNumber = 1 },
        };

        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = false },
                    },
                },
            },
        };

        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = false },
                    },
                },
            },
        };
        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(subsidiaryDetailsRequest);

        var service = CreateService(new ValidationSettings { ErrorLimit = 50 });

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(subsidiaryDetailsResponse);

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(1, totalErrors);
        Assert.AreEqual(1, validationErrors.Count);
        Assert.AreEqual(ErrorCodes.SubsidiaryIdDoesNotExist, validationErrors.First().ColumnErrors.First().ErrorCode);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_ShouldAddError_WhenSubsidiaryBelongsToDifferentOrganisation()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1", LineNumber = 1 },
        };

        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false },
                    },
                },
            },
        };

        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = true },
                    },
                },
            },
        };

        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(subsidiaryDetailsRequest);

        var service = CreateService(new ValidationSettings { ErrorLimit = 50 });

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(subsidiaryDetailsResponse);

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(1, totalErrors);
        Assert.AreEqual(1, validationErrors.Count);
        Assert.AreEqual(ErrorCodes.SubsidiaryIdBelongsToDifferentOrganisation, validationErrors.First().ColumnErrors.First().ErrorCode);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_ShouldAddError_WhenSubsidiaryDoesNotBelongToAnyOrganisation()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1", LineNumber = 1 },
        };

        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false, SubsidiaryDoesNotBelongToAnyOrganisation = false },
                    },
                },
            },
        };

        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false, SubsidiaryDoesNotBelongToAnyOrganisation = true },
                    },
                },
            },
        };

        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(subsidiaryDetailsRequest);

        var service = CreateService(new ValidationSettings { ErrorLimit = 50 });

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(subsidiaryDetailsResponse);

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(1, totalErrors);
        Assert.AreEqual(1, validationErrors.Count);
        Assert.AreEqual(ErrorCodes.SubsidiaryDoesNotBelongToAnyOrganisation, validationErrors.First().ColumnErrors.First().ErrorCode);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_ShouldStopValidation_WhenErrorLimitReached()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1", LineNumber = 1 },
            new() { DefraId = "ORG2", SubsidiaryId = "SUB2", LineNumber = 2 },
        };

        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = false },
                    },
                },
            },
        };
        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = true },
                    },
                },
            },
        };
        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(subsidiaryDetailsRequest);

        var service = CreateService(new ValidationSettings { ErrorLimit = 1 });

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(subsidiaryDetailsResponse);

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(1, totalErrors);
        Assert.AreEqual(1, validationErrors.Count);
        _companyDetailsApiClientMock.Verify(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_ShouldLogHttpRequestException()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1", LineNumber = 1 },
        };

        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(new SubsidiaryDetailsRequest() { SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>() { new SubsidiaryOrganisationDetail() } });
        var service = CreateService(new ValidationSettings { ErrorLimit = 1 });

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ThrowsAsync(new HttpRequestException());

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(0, totalErrors);
        Assert.AreEqual(0, validationErrors.Count);
        _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during Subsidiary validation")),
            It.IsAny<HttpRequestException>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
        Times.Once);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_ShouldReturnTotalErrorsAndValidationErrors_WhenRequestIsNull()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>();
        var totalErrors = 0;

        var service = CreateService(new ValidationSettings { ErrorLimit = 100 });
        _subsidiaryDetailsRequestBuilderMock
            .Setup(builder => builder.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns((SubsidiaryDetailsRequest)null); // Simulating null request

        // Act
        var result = await service.ValidateSubsidiary(rows, totalErrors, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(totalErrors, result.TotalErrors);
        Assert.AreEqual(0, result.ValidationErrors.Count);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_ShouldReturnTotalErrorsAndValidationErrors_WhenOrganisationDetailsIsNull()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>();
        var totalErrors = 0;

        var service = CreateService(new ValidationSettings { ErrorLimit = 100 });
        _subsidiaryDetailsRequestBuilderMock
            .Setup(builder => builder.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(new SubsidiaryDetailsRequest { SubsidiaryOrganisationDetails = null });

        // Act
        var result = await service.ValidateSubsidiary(rows, totalErrors, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(totalErrors, result.TotalErrors);
        Assert.AreEqual(0, result.ValidationErrors.Count);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_ShouldReturnTotalErrorsAndValidationErrors_WhenOrganisationDetailsIsEmpty()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>();
        var totalErrors = 0;

        var service = CreateService(new ValidationSettings { ErrorLimit = 100 });
        _subsidiaryDetailsRequestBuilderMock
            .Setup(builder => builder.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(new SubsidiaryDetailsRequest { SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>() });

        // Act
        var result = await service.ValidateSubsidiary(rows, totalErrors, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(totalErrors, result.TotalErrors);
        Assert.AreEqual(0, result.ValidationErrors.Count);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_SubsidiaryID_ShouldBelongToOrganisation()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB001", OrganisationName = "Subsidiary XZ01", LineNumber = 1 },
            new() { DefraId = "ORG1", SubsidiaryId = "SUB002", OrganisationName = "Subsidiary NM02", LineNumber = 2 },
        };

        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB001", SubsidiaryExists = false },
                        new() { ReferenceNumber = "SUB002", SubsidiaryExists = false },
                    },
                },
            },
        };

        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB001", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false, SubsidiaryDoesNotBelongToAnyOrganisation = false },
                        new() { ReferenceNumber = "SUB002", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false, SubsidiaryDoesNotBelongToAnyOrganisation = false },
                    },
                },
            },
        };

        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(subsidiaryDetailsRequest);

        var service = CreateService(new ValidationSettings { ErrorLimit = 1 });

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(subsidiaryDetailsResponse);

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(0, totalErrors);
        Assert.AreEqual(0, validationErrors.Count);
        _companyDetailsApiClientMock.Verify(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_SubsidiaryID_MatchesCompaniesHouseNumber()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB001", OrganisationName = "Subsidiary XZ01", CompaniesHouseNumber = "1001001", LineNumber = 1 },
            new() { DefraId = "ORG1", SubsidiaryId = "SUB002", OrganisationName = "Subsidiary NM02", CompaniesHouseNumber = "2002002", LineNumber = 2 },
        };

        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB001", SubsidiaryExists = false },
                        new() { ReferenceNumber = "SUB002", SubsidiaryExists = false },
                    },
                },
            },
        };

        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB001", CompaniesHouseNumber = "1001001", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false, SubsidiaryDoesNotBelongToAnyOrganisation = false },
                        new() { ReferenceNumber = "SUB002", CompaniesHouseNumber = "2002002", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false, SubsidiaryDoesNotBelongToAnyOrganisation = false },
                    },
                },
            },
        };

        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(subsidiaryDetailsRequest);

        var service = CreateService(new ValidationSettings { ErrorLimit = 1 });

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(subsidiaryDetailsResponse);

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(0, totalErrors);
        Assert.AreEqual(0, validationErrors.Count);
        _companyDetailsApiClientMock.Verify(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()), Times.Once);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_SubsidiaryID_DoesNotMatchCompaniesHouseNumber()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB001", OrganisationName = "Subsidiary XZ01", CompaniesHouseNumber = "1001001", LineNumber = 1 },
            new() { DefraId = "ORG1", SubsidiaryId = "SUB002", OrganisationName = "Subsidiary NM02", CompaniesHouseNumber = "2002002", LineNumber = 2 },
        };

        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB001", SubsidiaryExists = false },
                        new() { ReferenceNumber = "SUB002", SubsidiaryExists = false },
                    },
                },
            },
        };

        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB001", CompaniesHouseNumber = "1XX1XX1", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false, SubsidiaryDoesNotBelongToAnyOrganisation = false },
                        new() { ReferenceNumber = "SUB002", CompaniesHouseNumber = "2XX2XX2", SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false, SubsidiaryDoesNotBelongToAnyOrganisation = false },
                    },
                },
            },
        };

        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(subsidiaryDetailsRequest);

        var service = CreateService(new ValidationSettings { ErrorLimit = 100 });

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(subsidiaryDetailsResponse);

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(2, totalErrors);
        Assert.AreEqual(2, validationErrors.Count);
        _companyDetailsApiClientMock.Verify(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()), Times.Once);
    }

    [TestMethod]
    public async Task ValidateOrganisationsAsync_WithOutOrgSizeFieldInRow_WithOrgSizeFlagOn_ThrowException()
    {
        // Arrange
        const int rowCount = 4;
        const int maxErrors = 10;
        var organisationSizeFlag = true;
        var dataRows = RowDataTestHelper.GenerateOrgs_WithoutOrganisationSizeField(rowCount).ToArray();
        var service = CreateServiceWithOrganisationSizeFieldValidationToggle(organisationSizeFlag, new ValidationSettings { ErrorLimit = maxErrors });

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
        var errors = async () => await service.ValidateOrganisationsAsync(dataRows.ToList(), blobQueueMessage, false);

        // Assert
        errors.Should().ThrowAsync<ArgumentException>();

        _companyDetailsApiClientMock.Verify(
            m => m.GetCompanyDetailsByProducer(It.IsAny<string>()),
            Times.Never());
    }

    [TestMethod]
    [DataRow(true, true, 0)]
    [DataRow(true, false, 0)]
    [DataRow(false, false, 0)]
    public async Task ValidateOrganisationsAsync_WithOutOrgSizeFieldInRow_WithOrgSizeFlagOff_True(bool includeOrgSizeFieldInRows, bool orgSizeFeatureFlag, int errorCount)
    {
        // Arrange
        const int rowCount = 4;
        const int maxErrors = 10;
        var dataRows = includeOrgSizeFieldInRows ? RowDataTestHelper.GenerateOrgs(rowCount).ToArray() : RowDataTestHelper.GenerateOrgs_WithoutOrganisationSizeField(rowCount).ToArray();
        var service = CreateServiceWithOrganisationSizeFieldValidationToggle(orgSizeFeatureFlag, new ValidationSettings { ErrorLimit = maxErrors });

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
        errors.Count().Should().Be(errorCount);

        _companyDetailsApiClientMock.Verify(
            m => m.GetCompanyDetailsByProducer(It.IsAny<string>()),
            Times.Never());
    }

    [TestMethod]

    public async Task ValidateSubsidiary_ShouldNotAddError_WhenJoinerDateDoesNotMatchJoinerDateInDatabaseButThereIsAlreadyAJoinerDateError()
    {
        // Arrange
        var expectedErrorCodes = new string[] { };
        var existingErrors = new List<Data.Models.SubmissionApi.RegistrationValidationError>()
        {
            new RegistrationValidationError()
            {
                RowNumber = 1,
                ColumnErrors = new List<ColumnValidationError>(),
            },
        };

        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1", LineNumber = 1, JoinerDate = "04/11/1999" },
        };

        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = true, },
                    },
                },
            },
        };

        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new()
                        {
                            ReferenceNumber = "SUB1",
                            SubsidiaryExists = true,
                            SubsidiaryBelongsToAnyOtherOrganisation = false,
                            SubsidiaryDoesNotBelongToAnyOrganisation = false,
                        },
                    },
                },
            },
        };

        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(subsidiaryDetailsRequest);

        var service = CreateService(new ValidationSettings { ErrorLimit = 50 });

        _featureManagerMock
            .Setup(fm => fm.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns))
            .ReturnsAsync(true);

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(subsidiaryDetailsResponse);

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, existingErrors);

        // Assert
        Assert.AreEqual(0, totalErrors);
        Assert.AreEqual(0, validationErrors.Count);
        CollectionAssert.AreEquivalent(expectedErrorCodes, validationErrors.SelectMany(x => x.ColumnErrors).Select(x => x.ErrorCode).ToArray());
    }

    [TestMethod]
    [DataRow("1999/11/04", 0, true, false, false, new string[] { })]
    [DataRow("", 0, true, false, false, new string[] { })]
    [DataRow("1999/11/04", 0, true, false, false, new string[] { }, true)]
    [DataRow("1999/11/04", 0, true, false, false, new string[] { })]

    public async Task ValidateSubsidiary_ShouldNotAddError_WhenJoinerDateDoesNotMatchJoinerDateInDatabaseButFeatureIsTurnedOff(
        string joinerDate,
        int errorCount,
        bool subsidiaryExists,
        bool subsidiaryBelongsToAnyOtherOrganisation,
        bool subsidiaryDoesNotBelongToAnyOrganisation,
        string[] expectedErrorCodes,
        bool nullDateReturnedFromDB = false)
    {
        // Arrange
        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1", LineNumber = 1, JoinerDate = joinerDate },
        };

        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new() { ReferenceNumber = "SUB1", SubsidiaryExists = true },
                    },
                },
            },
        };

        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "ORG1",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new()
                        {
                            ReferenceNumber = "SUB1",
                            SubsidiaryExists = subsidiaryExists,
                            SubsidiaryBelongsToAnyOtherOrganisation = subsidiaryBelongsToAnyOtherOrganisation,
                            SubsidiaryDoesNotBelongToAnyOrganisation = subsidiaryDoesNotBelongToAnyOrganisation,
                        },
                    },
                },
            },
        };

        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(It.IsAny<List<OrganisationDataRow>>()))
            .Returns(subsidiaryDetailsRequest);

        var service = CreateService(new ValidationSettings { ErrorLimit = 50 });

        _featureManagerMock
            .Setup(fm => fm.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns))
            .ReturnsAsync(false);

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(subsidiaryDetailsResponse);

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, new List<Data.Models.SubmissionApi.RegistrationValidationError>());

        // Assert
        Assert.AreEqual(errorCount, totalErrors);
        Assert.AreEqual(errorCount, validationErrors.Count);
        CollectionAssert.AreEquivalent(expectedErrorCodes, validationErrors.SelectMany(x => x.ColumnErrors).Select(x => x.ErrorCode).ToArray());
    }

    [TestMethod]
    public async Task ValidateSubsidiary_ShouldNotAddJoinerDateMismatchError_WhenJoinerDateRequiredErrorExists()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1", LineNumber = 1, JoinerDate = "01/01/2023" },
        };

        var existingErrors = new List<RegistrationValidationError>
        {
            new() { RowNumber = 1, ColumnErrors = new List<ColumnValidationError>() },
        };

        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, existingErrors);

        // Assert
        Assert.IsFalse(validationErrors.Any(e => e.ColumnErrors.Any(ce => ce.ErrorCode == ErrorCodes.JoinerDateDoesNotMatchJoinerDateInDatabase)));
    }

    [TestMethod]
    public async Task ValidateSubsidiary_ShouldSkipValidation_WhenFeatureFlagIsDisabled()
    {
        // Arrange
        var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1", LineNumber = 1, JoinerDate = "01/01/2023", },
        };

        var existingErrors = new List<RegistrationValidationError>();

        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns))
            .ReturnsAsync(false);

        var service = CreateService();

        // Act
        var (totalErrors, validationErrors) = await service.ValidateSubsidiary(rows, 0, existingErrors);

        // Assert
        Assert.AreEqual(0, validationErrors.Count); // No validation should occur
    }

    [TestMethod]
    public async Task ValidateOrganisationsAsync_WithSubsidiaryId_WithValidJoinerDateFormat()
    {
        // Arrange
        var organisations = new List<OrganisationDataRow>
        {
            new OrganisationDataRow
            {
                SubsidiaryId = "1",
                JoinerDate = "01/01/2020",
            },
        };

        var service = CreateService();

        // Act
        var result = await service.ValidateOrganisationsAsync(organisations, new BlobQueueMessage(), false);

        // Assert
        result.Should().NotBeEmpty();
        result[0].ColumnErrors.Should().NotContain(x => x.ErrorCode == ErrorCodes.InvalidJoinerDateFormat);
    }

    [TestMethod]
    [DataRow("", "", "", "01/01/2000")]
    [DataRow("A", "01/01/2001", "test", "01/01/2000")]
    [DataRow("A", "01/01/2001", "", "")]
    public async Task ValidateOrganisationsAsync_WithValidJoinerLeaverDetailsCombination(
        string statusCode,
        string leaverDate,
        string organisationChangeReason,
        string joinerDate)
    {
        // Arrange
        var organisations = RowDataTestHelper.GenerateOrgIdSubId(1).ToList();

        organisations[0].StatusCode = statusCode;
        organisations[0].LeaverDate = leaverDate;
        organisations[0].OrganisationChangeReason = organisationChangeReason;
        organisations[0].JoinerDate = joinerDate;

        var service = CreateService();

        // Act
        var result = await service.ValidateOrganisationsAsync(organisations, new BlobQueueMessage(), false);

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("A", "", "", "", 1)]
    [DataRow("", "01/01/2001", "", "", 1)]
    public async Task ValidateOrganisationsAsync_WithInvalidJoinerLeaverDetailsCombination(
        string statusCode,
        string leaverDate,
        string organisationChangeReason,
        string joinerDate,
        int expectedErrorCount)
    {
        // Arrange
        var organisations = RowDataTestHelper.GenerateOrgIdSubId(1).ToList();

        organisations[0].StatusCode = statusCode;
        organisations[0].LeaverDate = leaverDate;
        organisations[0].OrganisationChangeReason = organisationChangeReason;
        organisations[0].JoinerDate = joinerDate;

        var service = CreateService();

        // Act
        var result = await service.ValidateOrganisationsAsync(organisations, new BlobQueueMessage(), false);

        // Assert
        result.Should().NotBeEmpty();
        result[0].ColumnErrors.Count.Should().Be(expectedErrorCount);
    }

    [TestMethod]
    public async Task ValidateOrganisationsAsync_WithSubsidiaryIdStatusCode_WithoutLeaverDate()
    {
        // Arrange
        var organisations = new List<OrganisationDataRow> { new OrganisationDataRow { SubsidiaryId = "1", StatusCode = "Any" } };
        var service = CreateService();

        // Act
        var result = await service.ValidateOrganisationsAsync(organisations, new BlobQueueMessage(), false);

        // Assert
        result.Should().NotBeEmpty();
        result[0].ColumnErrors.Should().Contain(x => x.ErrorCode == ErrorCodes.LeaverDateMustBePresentWhenStatusCodePresent);
    }

    [TestMethod]
    public async Task ValidateOrganisationsAsync_WithSubsidiaryIdStatusCode_WithInvalidLeaverDate()
    {
        // Arrange
        var organisations = new List<OrganisationDataRow>
        {
            new OrganisationDataRow
            {
                SubsidiaryId = "1",
                StatusCode = "Any",
                LeaverDate = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy"),
            },
        };

        var service = CreateService();

        // Act
        var result = await service.ValidateOrganisationsAsync(organisations, new BlobQueueMessage(), false);

        // Assert
        result.Should().NotBeEmpty();
        result[0].ColumnErrors.Should().Contain(x => x.ErrorCode == ErrorCodes.LeaverDateCannotBeInTheFuture);
    }

    [TestMethod]
    public async Task ValidateOrganisationsAsync_WithSubsidiaryIdStatusCode_WithOrganisationChangeReasonExceedingAllowedMaxLength()
    {
        // Arrange
        var organisations = new List<OrganisationDataRow>
        {
            new OrganisationDataRow
            {
                SubsidiaryId = "1",
                StatusCode = "Any",
                OrganisationChangeReason = new string('X', 201),
            },
        };
        var service = CreateService();

        // Act
        var result = await service.ValidateOrganisationsAsync(organisations, new BlobQueueMessage(), false);

        // Assert
        result.Should().NotBeEmpty();
        result[0].ColumnErrors.Should().Contain(x => x.ErrorCode == ErrorCodes.OrganisationChangeReasonCannotBeLongerThan200Characters);
    }

    [TestMethod]
    public async Task ValidateOrganisationsAsync_WithEmptyRegistrationTypeCode_OnCSUpload()
    {
        // Arrange
        var organisations = new List<OrganisationDataRow> { new OrganisationDataRow { RegistrationTypeCode = string.Empty } };
        var service = CreateService();

        // Act
        var result = await service.ValidateOrganisationsAsync(organisations, new BlobQueueMessage { ComplianceSchemeId = "1" }, false);

        // Assert
        result.Should().NotBeEmpty();
        result[0].ColumnErrors.Should().Contain(x => x.ErrorCode == ErrorCodes.RegistrationTypeCodeIsMandatoryCS);
    }

    [TestMethod]
    public async Task ValidateOrganisationsAsync_WithSubsidiaryIdStatusCode_WithOrganisationChangeReasonNotExceedingAllowedMaxLength()
    {
        // Arrange
        var organisations = new List<OrganisationDataRow>
        {
            new OrganisationDataRow
            {
                SubsidiaryId = "1",
                StatusCode = "Any",
                OrganisationChangeReason = new string('X', 200),
                LeaverDate = "01/01/2022",
            },
        };
        var service = CreateService();

        // Act
        var result = await service.ValidateOrganisationsAsync(organisations, new BlobQueueMessage(), false);

        // Assert
        result.SelectMany(x => x.ColumnErrors.Select(y => y.ErrorCode)).ToList()
            .Should().NotContain(new[] { ErrorCodes.OrganisationChangeReasonCannotBeLongerThan200Characters });
    }

    private ValidationService CreateService(ValidationSettings? settings = null)
    {
        var featureManageMock = new Mock<IFeatureManager>();
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation))
            .Returns(Task.FromResult(true));
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns))
            .Returns(Task.FromResult(true));

        _companyDetailsApiClientMock = new Mock<ICompanyDetailsApiClient>();

        var rowValidators = new RowValidators(
            new OrganisationDataRowValidator(featureManageMock.Object),
            new OrganisationDataRowWarningValidator(),
            new BrandDataRowValidator(),
            new PartnerDataRowValidator());

        return new ValidationService(
            rowValidators,
            new ColumnMetaDataProvider(featureManageMock.Object),
            Options.Create(settings ?? new ValidationSettings()),
            _companyDetailsApiClientMock.Object,
            _loggerMock.Object,
            _featureManagerMock.Object,
            _subsidiaryDetailsRequestBuilderMock.Object);
    }

    private ValidationService CreateServiceWithOrganisationSizeFieldValidationToggle(bool enableOrgSizeFieldValidation, ValidationSettings? settings = null)
    {
        var featureManageMock = new Mock<IFeatureManager>();
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation))
            .Returns(Task.FromResult(enableOrgSizeFieldValidation));

        _companyDetailsApiClientMock = new Mock<ICompanyDetailsApiClient>();

        var rowValidators = new RowValidators(
            new OrganisationDataRowValidator(featureManageMock.Object),
            new OrganisationDataRowWarningValidator(),
            new BrandDataRowValidator(),
            new PartnerDataRowValidator());

        return new ValidationService(
            rowValidators,
            new ColumnMetaDataProvider(featureManageMock.Object),
            Options.Create(settings ?? new ValidationSettings()),
            _companyDetailsApiClientMock.Object,
            _loggerMock.Object,
            _featureManagerMock.Object,
            _subsidiaryDetailsRequestBuilderMock.Object);
    }

    private class InvalidRowType : ICsvDataRow
    {
        public int LineNumber { get; set; }
    }
}