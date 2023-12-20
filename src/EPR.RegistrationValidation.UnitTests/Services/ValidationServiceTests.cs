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
        var results = await service.ValidateAsync(dataRows);

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
                AuditAddressCountry = AuditingCountryCodes.England,
            },
        };

        // Act
        var results = await service.ValidateAsync(dataRows);

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
        var results = await service.ValidateAsync(dataRows.ToList());

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
        var results = await service.ValidateAsync(dataRows.ToList());

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
        var results = await service.ValidateAsync(dataRows.ToList());

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
        var results = await service.ValidateAsync(dataRows.ToList());

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
    public async Task OrganisationMainActivitySicValidation_WhenMainActivitySicInvalid_ThenError()
    {
        // Arrange
        var dataRow = RowDataTestHelper.GenerateOrgs(1).First();
        var service = CreateService();

        dataRow.MainActivitySic = "123456";

        // Act
        var errors = await service.ValidateAsync(new[] { dataRow });

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
        var errors = await service.ValidateAsync(new[] { dataRow });

        // Assert
        var columnError = errors.Single().ColumnErrors.Single();

        columnError.ColumnName.Should().Be("trading_name");
        columnError.ErrorCode.Should().Be(ErrorCodes.TradingNameSameAsOrganisationName);
    }

    private static ValidationService CreateService(ValidationSettings? settings = null)
    {
        return new ValidationService(
            new OrganisationDataRowValidator(),
            new ColumnMetaDataProvider(),
            Options.Create(settings ?? new ValidationSettings()),
            Mock.Of<ILogger<ValidationService>>());
    }
}