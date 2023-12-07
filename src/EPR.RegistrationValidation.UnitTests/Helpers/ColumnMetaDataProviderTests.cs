namespace EPR.RegistrationValidation.UnitTests.Helpers;

using EPR.RegistrationValidation.Application.Helpers;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ColumnMetaDataProviderTests
{
    [TestMethod]
    public void ListColumnsMetaData_FromCsvModelClass_ReturnsPopulatedList()
    {
        // Arrange
        var metaDataProvider = new ColumnMetaDataProvider();

        // Act
        var columnMetaData = metaDataProvider.ListColumnMetaData<OrganisationDataRow>();

        // Assert
        columnMetaData.Should().NotBeEmpty();
        columnMetaData.ContainsKey(nameof(OrganisationDataRow.DefraId)).Should().BeTrue();
        columnMetaData[nameof(OrganisationDataRow.DefraId)].Index.Should().BeGreaterThan(-1);
        columnMetaData[nameof(OrganisationDataRow.DefraId)].Name.Should().NotBeEmpty();
    }

    [TestMethod]
    public void ListColumnsMetaData_FromClassWithoutAttributes_ReturnsEmptyList()
    {
        // Arrange
        var metaDataProvider = new ColumnMetaDataProvider();

        // Act
        var columnMetaData = metaDataProvider.ListColumnMetaData<TestModel>();

        // Assert
        columnMetaData.Should().BeEmpty();
    }

    [TestMethod]
    public void GetOrganisationColumnMetaData_ReturnsCorrectMetaData()
    {
        // Arrange
        var metaDataProvider = new ColumnMetaDataProvider();

        // Act
        var metaData = metaDataProvider.GetOrganisationColumnMetaData(nameof(OrganisationDataRow.DefraId));

        // Assert
        metaData.Should().NotBeNull();
        metaData.Index.Should().BeGreaterThan(-1);
        metaData.Name.Should().NotBeEmpty();
    }

    private class TestModel
    {
        public string NoAttribute { get; set; }
    }
}