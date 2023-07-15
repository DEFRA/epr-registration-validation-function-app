namespace EPR.RegistrationValidation.UnitTests.Helpers;

using Application.Exceptions;
using Application.Helpers;
using Data.Enums;
using Data.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SampleData;
using TestHelpers;

[TestClass]
public class CSVStreamParserTests
{
    private CsvStreamParser _sut;

    public CSVStreamParserTests(CsvStreamParser sut)
    {
        _sut = sut;
    }

    [TestMethod]
    public async Task TestGetItemsFromCsvStream_WhenStreamIsValid_ReturnsList()
    {
        // ARRANGE
        var csvDataRow = CSVRowTestHelper.GenerateCSVDataRowTestHelper(RequiredOrganisationTypeCodeForPartners.PAR.ToString(), RequiredPackagingActivityForBrands.Primary.ToString());
        var csvDataRow2 = CSVRowTestHelper.GenerateCSVDataRowTestHelper(RequiredOrganisationTypeCodeForPartners.LLP.ToString(), RequiredPackagingActivityForBrands.Secondary.ToString());
        var csvDataRows = new List<CsvDataRow>
        {
            csvDataRow,
            csvDataRow2,
        };

        var fileString = SampleCompanyData.GenerateDummyFileString(csvDataRows);
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(fileString);
        await writer.FlushAsync();
        stream.Position = 0;

        // ACT
        var response = _sut.GetItemsFromCsvStream<CsvDataRow>(stream);

        // ASSERT
        response[0].OrganisationTypeCode.Should().Be(RequiredOrganisationTypeCodeForPartners.PAR.ToString());
        response[0].PackagingActivitySO.Should().Be(RequiredPackagingActivityForBrands.Primary.ToString());
        response[1].OrganisationTypeCode.Should().Be(RequiredOrganisationTypeCodeForPartners.LLP.ToString());
        response[1].PackagingActivitySO.Should().Be(RequiredPackagingActivityForBrands.Secondary.ToString());
    }

    [TestMethod]
    public async Task TestGetItemsFromCsvStream_WhenStreamIsInvalid_ThrowsNewCsvParseException()
    {
        // ARRANGE
        var fileString = @"error,error,error
error,
error, error";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(fileString);
        await writer.FlushAsync();
        stream.Position = 0;

        // ACT
        Action act = () => _sut.GetItemsFromCsvStream<CsvDataRow>(stream);

        // ASSERT
        act.Should().Throw<CsvParseException>();
    }
}