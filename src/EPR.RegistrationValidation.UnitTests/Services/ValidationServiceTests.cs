namespace EPR.RegistrationValidation.UnitTests.Services;

using EPR.RegistrationValidation.Application.Constants;
using EPR.RegistrationValidation.Application.Helpers;
using EPR.RegistrationValidation.Application.Services;
using EPR.RegistrationValidation.Application.Validators;
using EPR.RegistrationValidation.Data.Config;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using EPR.RegistrationValidation.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class ValidationServiceTests
{
    [TestMethod]
    public async Task Validate_WithoutOrgId_ExpectOrgIdValidationError()
    {
        // Arrange
        const int expectedRow = 0;
        const int expectedColumnIndex = 0;
        const string expectedColumnName = "organisation_id";
        var service = CreateService();
        var dataRows = new List<OrganisationDataRow> { new() };

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows);

        // Assert
        var validationError = results.First(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.MissingOrganisationId));
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
            },
        };

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows);

        // Assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithManyValidRows_AndRowCountGreaterThanErrorLimit_ExpectNoValidationErrors()
    {
        // Arrange
        int rowCount = 20;
        int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount);
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows.ToList());

        // Assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithNoDuplicateOrganisationIdSubsidiaryId_ExpectNoValidationErrors()
    {
        // Arrange
        int rowCount = 6;
        int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgIdSubId(rowCount);
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows.ToList());

        // Assert
        results.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Validate_WithDuplicateOrganisationIdSubsidiaryId_ExpectValidationErrors()
    {
        // Arrange
        int rowCount = 6;
        int maxErrors = 20;
        var dataRows = RowDataTestHelper.GenerateDuplicateOrgIdSubId(rowCount);

        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows.ToList());

        // Assert
        var validationError = results.First(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.DuplicateOrganisationIdSubsidiaryId));
        validationError.Should().NotBeNull();
    }

    [TestMethod]
    public async Task Validate_WithDuplicateRows_AndRowCountGreaterThanErrorLimit_ReturnsLimitedErrors()
    {
        // Arrange
        int rowCount = 20;
        int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateDuplicateOrgIdSubId(rowCount);
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = await service.ValidateOrganisationsAsync(dataRows.ToList());

        // Assert
        results.Sum(x => x.ColumnErrors.Count).Should().Be(maxErrors);
    }

    [TestMethod]
    public async Task ValidateRowsAsync_WithInvalidRows_AndRowCountGreaterThanErrorLimit_ReturnsLimitedErrors()
    {
        // Arrange
        int rowCount = 20;
        int maxErrors = 10;
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
        int rowCount = 20;
        int maxErrors = 10;
        int initialTotalErrors = 5;
        int expectedDuplicateErrors = 5;
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
        int rowCount = 20;
        int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].OrganisationSubTypeCode = OrganisationSubTypeCodes.Licensor;
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = service.ValidateOrganisationSubType(dataRows.ToList(), 0);

        // Assert
        var validationError = results.ValidationErrors.First(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.HeadOrganisationMissingSubOrganisation));
        validationError.Should().NotBeNull();
    }

    [TestMethod]
    public async Task ValidateOrganisationSubType_WithSubOrganisationTypeAndEmptySubsidiaryID_ReturnsRowError()
    {
        // Arrange
        int rowCount = 20;
        int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].OrganisationSubTypeCode = OrganisationSubTypeCodes.Licensor;
        dataRows[1].DefraId = dataRows[0].DefraId;
        dataRows[1].OrganisationSubTypeCode = OrganisationSubTypeCodes.Tenant;
        dataRows[1].SubsidiaryId = string.Empty;
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = service.ValidateOrganisationSubType(dataRows.ToList(), 0);

        // Assert
        var validationError = results.ValidationErrors.First(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.HeadOrganisationMissingSubOrganisation));
        validationError.Should().NotBeNull();
    }

    [TestMethod]
    public async Task ValidateOrganisationSubType_WithSubOrganisationType_ExpectNoValidationErrors()
    {
        // Arrange
        int rowCount = 20;
        int maxErrors = 10;
        var dataRows = RowDataTestHelper.GenerateOrgs(rowCount).ToArray();
        dataRows[0].OrganisationSubTypeCode = OrganisationSubTypeCodes.Licensor;
        dataRows[1].DefraId = dataRows[0].DefraId;
        dataRows[1].OrganisationSubTypeCode = OrganisationSubTypeCodes.Tenant;
        dataRows[1].SubsidiaryId = "54321";
        var service = CreateService(new ValidationSettings { ErrorLimit = maxErrors });

        // Act
        var results = service.ValidateOrganisationSubType(dataRows.ToList(), 0);

        // Assert
        var validationError = results.ValidationErrors.FirstOrDefault(x => x.ColumnErrors.Any(e => e.ErrorCode == ErrorCodes.HeadOrganisationMissingSubOrganisation));
        validationError.Should().BeNull();
    }

    [TestMethod]
    public async Task OrganisationMainActivitySicValidation_WhenMainActivitySicInvalid_ThenError()
    {
        // Arrange
        var dataRow = RowDataTestHelper.GenerateOrgs(1).First();
        var service = CreateService();

        dataRow.MainActivitySic = "123456";

        // Act
        var errors = await service.ValidateOrganisationsAsync(new List<OrganisationDataRow>() { dataRow });

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

        // Act
        var errors = await service.ValidateOrganisationsAsync(new List<OrganisationDataRow>() { dataRow });

        // Assert
        var columnError = errors.Single().ColumnErrors.Single();

        columnError.ColumnName.Should().Be("trading_name");
        columnError.ErrorCode.Should().Be(ErrorCodes.TradingNameSameAsOrganisationName);
    }

    [TestMethod]
    public async Task Validate_BrandFile_ExpectNoValidationError()
    {
        // Arrange
        int rowCount = 10;
        var dataRows = RowDataTestHelper.GenerateBrand(rowCount);
        var service = CreateService();

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows.ToList());

        // Assert
        results.Should().BeEmpty();
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
        var results = await service.ValidateAppendedFileAsync(dataRows);

        // Assert
        results.Should().NotBeNull();
        results[0].Should().Be(ErrorCodes.CharacterLengthExceeded);
    }

    [TestMethod]
    public async Task Validate_PartnerFile_ExpectNoValidationError()
    {
        // Arrange
        int rowCount = 10;
        var dataRows = RowDataTestHelper.GeneratePartner(rowCount);
        var service = CreateService();

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows.ToList());

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
        var results = await service.ValidateAppendedFileAsync(dataRows);

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
        var act = async () => await service.ValidateAppendedFileAsync(invalidRowTypes.ToList());

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    public async Task ValidateAppendedFileAsync_WithSameErrorInMultipleRows_OnlyAddsErrorOnce()
    {
        // Arrange
        int rowCount = 2;
        var dataRows = RowDataTestHelper.GeneratePartner(rowCount).ToList();
        dataRows[0].DefraId = new string('a', CharacterLimits.MaxLength + 1);
        dataRows[1].DefraId = new string('b', CharacterLimits.MaxLength + 1);
        var service = CreateService();

        // Act
        var results = await service.ValidateAppendedFileAsync(dataRows);

        // Assert
        results.Should().NotBeNull();
        results[0].Should().Be(ErrorCodes.CharacterLengthExceeded);
        results.Count.Should().Be(1);
    }

    private static ValidationService CreateService(ValidationSettings? settings = null)
    {
        return new ValidationService(
            new OrganisationDataRowValidator(),
            new BrandDataRowValidator(),
            new PartnerDataRowValidator(),
            new ColumnMetaDataProvider(),
            Options.Create(settings ?? new ValidationSettings()),
            Mock.Of<ILogger<ValidationService>>());
    }

    private class InvalidRowType : ICsvDataRow
    {
        public int LineNumber { get; set; }
    }
}