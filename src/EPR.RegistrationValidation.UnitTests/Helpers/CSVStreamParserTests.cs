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

    [TestInitialize]
    public void Setup()
    {
        var featureManageMock = new Mock<IFeatureManager>();
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation))
            .Returns(Task.FromResult(true));
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns))
            .Returns(Task.FromResult(true));
        featureManageMock
           .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableStatusCodeColumn))
           .Returns(Task.FromResult(true));

        _sut = new CsvStreamParser(new ColumnMetaDataProvider(featureManageMock.Object), featureManageMock.Object);
    }

    [TestMethod]
    [DataRow(true, DisplayName = "UseMinimalClassMapsEnabled")]
    [DataRow(false, DisplayName = "UseMinimalClassMapsDisabled")]
    public async Task TestGetItemsFromCsvStream_WhenStreamIsValid_ReturnsList(bool useMinimalClassMaps)
    {
        // Arrange
        var memoryStream = CsvFileReader.ReadFile("ValidFileWithCorrectHeaders.csv");

        // Act
        var response = await _sut.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream, useMinimalClassMaps);

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
    [DataRow(true, DisplayName = "UseMinimalClassMapsEnabled")]
    [DataRow(false, DisplayName = "UseMinimalClassMapsDisabled")]
    public async Task GetItemsFromCsvStream_WhenCsvHeaderIsValid_ValidateExpectedItemCount(bool useMinimalClassMaps)
    {
        // Arrange
        using var memoryStream = CsvFileReader.ReadFile("ValidFileWithCorrectHeaders.csv");

        // Act
        var items = await _sut.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream, useMinimalClassMaps);

        // Assert
        items.Should().HaveCount(2);
    }

    [TestMethod]
    [DataRow(true, DisplayName = "UseMinimalClassMapsEnabled")]
    [DataRow(false, DisplayName = "UseMinimalClassMapsDisabled")]
    public async Task GetItemsFromCsvStream_WithValidCsv_ValidateLineNumbers(bool useMinimalClassMaps)
    {
        // Arrange
        using var memoryStream = CsvFileReader.ReadFile("ValidFileWithCorrectHeaders.csv");

        // Act
        var items = await _sut.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream, useMinimalClassMaps);

        // Assert
        items.Should().HaveCount(2);
        items.Should().Contain(x => x.LineNumber == 2);
        items.Should().Contain(x => x.LineNumber == 3);
    }

    [TestMethod]
    public async Task GetItemsFromCsvStream_WhenCsvHeaderIsValid_ValidateMinimalPropertiesAreMapped()
    {
        // Arrange
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
    [DataRow(true, DisplayName = "UseMinimalClassMapsEnabled")]
    [DataRow(false, DisplayName = "UseMinimalClassMapsDisabled")]
    public async Task GetItemsFromCsvStream_WithBrandFile_ValidatePropertiesAreMapped(bool useMinimalClassMaps)
    {
        // Arrange
        string defraId = "145879";
        string subsidiaryId = "123456";
        string brandName = "Brand Name";
        string brandTypeCode = "Brand Type Code";

        // Build csv string with header
        string header = "organisation_id,subsidiary_id,brand_name,brand_type_code";
        var csvString = $"{header}{Environment.NewLine}{defraId},{subsidiaryId},{brandName},{brandTypeCode}";
        using var memoryStream = CsvFileReader.ReadString(csvString);

        // Act
        var items = await _sut.GetItemsFromCsvStreamAsync<BrandDataRow>(memoryStream, useMinimalClassMaps);

        // Assert
        items[0].DefraId.Should().Be(defraId);
        items[0].SubsidiaryId.Should().Be(subsidiaryId);
        items[0].BrandName.Should().Be(brandName);
        items[0].BrandTypeCode.Should().Be(brandTypeCode);
    }

    [TestMethod]
    [DataRow(true, DisplayName = "UseMinimalClassMapsEnabled")]
    [DataRow(false, DisplayName = " Disabled")]
    public async Task GetItemsFromCsvStream_WithPartnerFile_ValidatePropertiesAreMapped(bool useMinimalClassMaps)
    {
        // Arrange
        string defraId = "145879";
        string subsidiaryId = "123456";
        string partnerFirstName = "Partner First Name";
        string partnerLastName = "Partner Last Name";
        string partnerPhoneNumber = "Partner Phone Number";
        string partnerEmail = "Partner Email";

        // Build csv string with header
        string header = "organisation_id,subsidiary_id,partner_first_name,partner_last_name,partner_phone_number,partner_email";
        var csvString = $"{header}{Environment.NewLine}{defraId},{subsidiaryId},{partnerFirstName},{partnerLastName},{partnerPhoneNumber},{partnerEmail}";
        using var memoryStream = CsvFileReader.ReadString(csvString);

        // Act
        var items = await _sut.GetItemsFromCsvStreamAsync<PartnersDataRow>(memoryStream, useMinimalClassMaps);

        // Assert
        items[0].DefraId.Should().Be(defraId);
        items[0].SubsidiaryId.Should().Be(subsidiaryId);
        items[0].PartnerFirstName.Should().Be(partnerFirstName);
        items[0].PartnerLastName.Should().Be(partnerLastName);
        items[0].PartnerPhoneNumber.Should().Be(partnerPhoneNumber);
        items[0].PartnerEmail.Should().Be(partnerEmail);
    }

    [TestMethod]
    [DataRow(false, "ValidFileWithCorrectHeaders.csv")]
    [DataRow(true, "InvalidFileTooFewHeaders.csv")]
    public async Task TestGetItemsFromCsvStream_FeatureFlag_EnableOrganisationSizeFieldValidation_Toggle_WhenFileDoesNotMatch_ThrowsException(bool featureFlag, string csvFile)
    {
        // Arrange
        var featureManageMock = CreateFeatureManagerMock(featureFlag);

        featureManageMock
           .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableStatusCodeColumn))
           .Returns(Task.FromResult(true));

        var sutLocal = new CsvStreamParser(new ColumnMetaDataProvider(featureManageMock.Object), featureManageMock.Object);
        var memoryStream = CsvFileReader.ReadFile(csvFile);

        // Act
        Func<Task> act = () => sutLocal.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream);

        // Assert
        await act.Should().ThrowAsync<CsvHeaderException>().WithMessage("The CSV file header is invalid.");
    }

    [TestMethod]
    [DataRow(true, true, "ValidFileWithCorrectHeaders.csv", 2)]
    [DataRow(true, false, "ValidFileWithCorrectHeadersWithOldLeaverCodeColumnName.csv", 2)]
    [DataRow(false, true, "InvalidFileTooFewHeaders.csv", 2)]
    [DataRow(false, false, "InvalidFileTooFewHeaders.csv", 2)]
    public async Task TestGetItemsFromCsvStream_FeatureFlag_EnableOrganisationSizeFieldValidation_And_EnableSubsidiaryJoinerAndLeaverColumns_True_WhenCsvFileContainsSizeColumn_ReturnValidList(bool joinerLeaverFeatureFlag, bool columnNameFeatureFlag, string csvFile, int expectedResultCount)
    {
        // Arrange
        var featureManageMock = CreateFeatureManagerMock(joinerLeaverFeatureFlag);

        featureManageMock
           .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableStatusCodeColumn))
           .Returns(Task.FromResult(columnNameFeatureFlag));

        var sutLocal = new CsvStreamParser(new ColumnMetaDataProvider(featureManageMock.Object), featureManageMock.Object);
        var memoryStream = CsvFileReader.ReadFile(csvFile);

        // Act
        var result = sutLocal.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream).Result;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedResultCount, result.Count);
    }

    [TestMethod]
    [DataRow(true, "ValidFileWithCorrectHeaders.csv", 2)]
    [DataRow(false, "InvalidFileTooFewHeaders.csv", 2)]
    public async Task TestGetItemsFromCsvStream_FeatureFlag_EnableStatusCodeColumn_And_EnableOrganisationSizeFieldValidation_And_EnableSubsidiaryJoinerAndLeaverColumns_True_WhenCsvFileContainsSizeColumn_ReturnValidList(bool featureFlag, string csvFile, int expectedResultCount)
    {
        // Arrange
        var featureManageMock = CreateFeatureManagerMock(featureFlag);

        featureManageMock
           .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableStatusCodeColumn))
           .Returns(Task.FromResult(true));

        var sutLocal = new CsvStreamParser(new ColumnMetaDataProvider(featureManageMock.Object), featureManageMock.Object);
        var memoryStream = CsvFileReader.ReadFile(csvFile);

        // Act
        var result = sutLocal.GetItemsFromCsvStreamAsync<OrganisationDataRow>(memoryStream).Result;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedResultCount, result.Count);
    }

    private static Mock<IFeatureManager> CreateFeatureManagerMock(bool featureFlag)
    {
        var featureManageMock = new Mock<IFeatureManager>();
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation))
            .Returns(Task.FromResult(featureFlag));
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns))
            .Returns(Task.FromResult(featureFlag));

        return featureManageMock;
    }
}