namespace EPR.RegistrationValidation.UnitTests.Helpers;

using Application.Exceptions;
using Application.Helpers;
using Data.Enums;
using Data.Models;
using EPR.RegistrationValidation.Data.Constants;
using FluentAssertions;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestHelpers;

[TestClass]
public class CsvStreamParserTests
{
    private CsvStreamParser _sut;
    private Mock<IFeatureManager> _featureManagerMock;

    [TestInitialize]
    public void Setup()
    {
        _featureManagerMock = new Mock<IFeatureManager>();
        _sut = new CsvStreamParser(new ColumnMetaDataProvider(), _featureManagerMock.Object);
    }

    [TestMethod]
    [DataRow(true, DisplayName = "RowValidationEnabled")]
    [DataRow(false, DisplayName = "RowValidationDisabled")]
    public async Task TestGetItemsFromCsvStream_WhenStreamIsValid_ReturnsList(bool rowValidationEnabled)
    {
        // Arrange
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(rowValidationEnabled);

        var memoryStream = CsvFileReader.ReadFile("ValidFileWithCorrectHeaders.csv");

        // Act
        var response = await _sut.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream);

        // Assert
        response[0].OrganisationTypeCode.Should().Be(RequiredOrganisationTypeCodeForPartners.PAR.ToString());
        response[0].PackagingActivitySO.Should().Be(RequiredPackagingActivityForBrands.Primary.ToString());
        response[1].OrganisationTypeCode.Should().Be(RequiredOrganisationTypeCodeForPartners.LLP.ToString());
        response[1].PackagingActivitySO.Should().Be(RequiredPackagingActivityForBrands.Secondary.ToString());
    }

    [TestMethod]
    public async Task TestGetItemsFromCsvStream_WhenStreamIsInvalid_ThrowsNewCsvParseException()
    {
        // Arrange
        var fileString = @"error,error,error
error,
error, error";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(fileString);
        await writer.FlushAsync();
        stream.Position = 0;

        // Act
        Func<Task> act = () => _sut.GetItemsFromCsvStreamAsync<OrganisationDataRow>(stream);

        // Assert
        await act.Should().ThrowAsync<CsvHeaderException>();
    }

    [TestMethod]
    [DataRow("InvalidFileWithIncorrectHeaderName.csv")]
    public async Task TestGetItemsFromCsvStream_WhenCsvHeaderValueIsIncorrect_ThrowsNewCsvHeaderException(string invalidCsvFile)
    {
        // Arrange
        var memoryStream = CsvFileReader.ReadFile(invalidCsvFile);

        // Act
        Func<Task> act = () => _sut.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream);

        // Assert
        await act.Should().ThrowAsync<CsvHeaderException>().WithMessage("The CSV file header is invalid.");
    }

    [TestMethod]
    public async Task TestGetItemsFromCsvStream_WhenCsvHasTooManyHeaders_ThrowsNewCsvHeaderException()
    {
        // Arrange
        var memoryStream = CsvFileReader.ReadFile("InvalidFileTooManyHeaders.csv");

        // Act
        Func<Task> act = () => _sut.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream);

        // Assert
        await act.Should().ThrowAsync<CsvHeaderException>().WithMessage("The CSV file header is invalid.");
    }

    [TestMethod]
    public async Task TestGetItemsFromCsvStream_WhenCsvHasTooFewHeaders_ThrowsNewCsvHeaderException()
    {
        // Arrange
        var memoryStream = CsvFileReader.ReadFile("InvalidFileTooFewHeaders.csv");

        // Act
        Func<Task> act = () => _sut.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream);

        // Assert
        await act.Should().ThrowAsync<CsvHeaderException>().WithMessage("The CSV file header is invalid.");
    }

    [TestMethod]
    public async Task GetItemsFromCsvStream_WhenCsvHeaderOrderIsIncorrect_ThrowsNewCsvHeaderException()
    {
        // Arrange
        var memoryStream = CsvFileReader.ReadFile("FileWithIncorrectHeaderOrder.csv");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream);
        };

        // Assert
        await act.Should().ThrowAsync<CsvHeaderException>().WithMessage("The CSV file header is invalid.");
    }

    [TestMethod]
    [DataRow(true, DisplayName = "RowValidationEnabled")]
    [DataRow(false, DisplayName = "RowValidationDisabled")]
    public async Task GetItemsFromCsvStream_WhenCsvHeaderIsValid_ValidateExpectedItemCount(bool rowValidationEnabled)
    {
        // Arrange
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(rowValidationEnabled);

        using var memoryStream = CsvFileReader.ReadFile("ValidFileWithCorrectHeaders.csv");

        // Act
        var items = await _sut.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream);

        // Assert
        items.Should().HaveCount(2);
    }

    [TestMethod]
    [DataRow(true, DisplayName = "RowValidationEnabled")]
    [DataRow(false, DisplayName = "RowValidationDisabled")]
    public async Task GetItemsFromCsvStream_WithValidCsv_ValidateLineNumbers(bool rowValidationEnabled)
    {
        // Arrange
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(rowValidationEnabled);

        using var memoryStream = CsvFileReader.ReadFile("ValidFileWithCorrectHeaders.csv");

        // Act
        var items = await _sut.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream);

        // Assert
        items.Should().HaveCount(2);
        items.Should().Contain(x => x.LineNumber == 2);
        items.Should().Contain(x => x.LineNumber == 3);
    }

    [TestMethod]
    public async Task GetItemsFromCsvStream_WhenCsvHeaderIsValid_ValidateMinimalPropertiesAreMapped()
    {
        // Arrange
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(false);

        using var memoryStream = CsvFileReader.ReadFile("ValidFileWithCorrectHeaders.csv");

        // Act
        var items = await _sut.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream);

        // Assert minimal set of properties are not empty
        foreach (var row in items)
        {
            row.DefraId.Should().NotBeNullOrEmpty();
            row.SubsidiaryId.Should().NotBeNullOrEmpty();
            row.OrganisationTypeCode.Should().NotBeNullOrEmpty();
            row.PackagingActivitySO.Should().NotBeNullOrEmpty();
        }
    }

    [TestMethod]
    [DataRow(true, DisplayName = "RowValidationEnabled")]
    [DataRow(false, DisplayName = "RowValidationDisabled")]
    public async Task GetItemsFromCsvStream_WithBrandFile_ValidatePropertiesAreMapped(bool rowValidationEnabled)
    {
        // Arrange
        string defraId = "145879";
        string subsidiaryId = "123456";
        string brandName = "Brand Name";
        string brandTypeCode = "Brand Type Code";

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(rowValidationEnabled);

        // Build csv string with header
        string header = "organisation_id,subsidiary_id,brand_name,brand_type_code";
        var csvString = $"{header}{Environment.NewLine}{defraId},{subsidiaryId},{brandName},{brandTypeCode}";
        using var memoryStream = CsvFileReader.ReadString(csvString);

        // Act
        var items = await _sut.GetItemsFromCsvStreamAsync<BrandDataRow>(memoryStream);

        // Assert
        foreach (var row in items)
        {
            items.First().DefraId.Should().Be(defraId);
            items.First().SubsidiaryId.Should().Be(subsidiaryId);
            items.First().BrandName.Should().Be(brandName);
            items.First().BrandTypeCode.Should().Be(brandTypeCode);
        }
    }

    [TestMethod]
    [DataRow(true, DisplayName = "RowValidationEnabled")]
    [DataRow(false, DisplayName = "RowValidationDisabled")]
    public async Task GetItemsFromCsvStream_WithPartnerFile_ValidatePropertiesAreMapped(bool rowValidationEnabled)
    {
        // Arrange
        string defraId = "145879";
        string subsidiaryId = "123456";
        string partnerFirstName = "Partner First Name";
        string partnerLastName = "Partner Last Name";
        string partnerPhoneNumber = "Partner Phone Number";
        string partnerEmail = "Partner Email";

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.EnableRowValidation))
            .ReturnsAsync(rowValidationEnabled);

        // Build csv string with header
        string header = "organisation_id,subsidiary_id,partner_first_name,partner_last_name,partner_phone_number,partner_email";
        var csvString = $"{header}{Environment.NewLine}{defraId},{subsidiaryId},{partnerFirstName},{partnerLastName},{partnerPhoneNumber},{partnerEmail}";
        using var memoryStream = CsvFileReader.ReadString(csvString);

        // Act
        var items = await _sut.GetItemsFromCsvStreamAsync<PartnersDataRow>(memoryStream);

        // Assert
        foreach (var row in items)
        {
            items.First().DefraId.Should().Be(defraId);
            items.First().SubsidiaryId.Should().Be(subsidiaryId);
            items.First().PartnerFirstName.Should().Be(partnerFirstName);
            items.First().PartnerLastName.Should().Be(partnerLastName);
            items.First().PartnerPhoneNumber.Should().Be(partnerPhoneNumber);
            items.First().PartnerEmail.Should().Be(partnerEmail);
        }
    }
}