namespace EPR.RegistrationValidation.Data.UnitTests.Models;

using Data.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CsvDataRowTests
{
    [TestMethod]
    public void CsvDataRow_PropertyValidation_ReturnsExpectedResults()
    {
        // Arrange
        var csvDataRow = new CsvDataRow
        {
            DefraId = "123",
            SubsidiaryId = "456",
            OrganisationTypeCode = "OrgType1",
            PackagingActivitySO = "PackActivity1",
        };

        // Act
        var defraId = csvDataRow.DefraId;
        var subsidiaryId = csvDataRow.SubsidiaryId;
        var organisationTypeCode = csvDataRow.OrganisationTypeCode;
        var packagingActivitySO = csvDataRow.PackagingActivitySO;

        // Assert
        defraId.Should().Be("123");
        subsidiaryId.Should().Be("456");
        organisationTypeCode.Should().Be("OrgType1");
        packagingActivitySO.Should().Be("PackActivity1");
    }
}