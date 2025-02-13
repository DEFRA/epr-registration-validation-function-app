namespace EPR.RegistrationValidation.UnitTests.Helpers;

using EPR.RegistrationValidation.Application.Helpers;
using EPR.RegistrationValidation.Data.Constants;
using EPR.RegistrationValidation.Data.Models;
using FluentAssertions;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class ColumnMetaDataProviderTests
{
    [TestMethod]
    public void ListColumnsMetaData_FromCsvModelClass_ReturnsPopulatedList()
    {
        // Arrange
        var featureManageMock = new Mock<IFeatureManager>();
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation))
            .Returns(Task.FromResult(false));
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns))
            .Returns(Task.FromResult(false));

        var metaDataProvider = new ColumnMetaDataProvider(featureManageMock.Object);

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
        var featureManageMock = new Mock<IFeatureManager>();
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation))
            .Returns(Task.FromResult(false));
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns))
            .Returns(Task.FromResult(false));

        var metaDataProvider = new ColumnMetaDataProvider(featureManageMock.Object);

        // Act
        var columnMetaData = metaDataProvider.ListColumnMetaData<TestModel>();

        // Assert
        columnMetaData.Should().BeEmpty();
    }

    [TestMethod]
    public void GetOrganisationColumnMetaData_ReturnsCorrectMetaData()
    {
        // Arrange
        var featureManageMock = new Mock<IFeatureManager>();
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableOrganisationSizeFieldValidation))
            .Returns(Task.FromResult(false));
        featureManageMock
            .Setup(m => m.IsEnabledAsync(FeatureFlags.EnableSubsidiaryJoinerAndLeaverColumns))
            .Returns(Task.FromResult(false));

        var metaDataProvider = new ColumnMetaDataProvider(featureManageMock.Object);

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