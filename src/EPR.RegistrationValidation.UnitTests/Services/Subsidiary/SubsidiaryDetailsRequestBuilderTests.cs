namespace EPR.RegistrationValidation.UnitTests.Services.Subsidiary
{
    using EPR.RegistrationValidation.Application.Services.Subsidiary;
    using EPR.RegistrationValidation.Data.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SubsidiaryDetailsRequestBuilderTests
    {
        private SubsidiaryDetailsRequestBuilder _subsidiaryDetailsRequestBuilder;

        [TestInitialize]
        public void Setup()
        {
            _subsidiaryDetailsRequestBuilder = new SubsidiaryDetailsRequestBuilder();
        }

        [TestMethod]
        public void CreateRequest_ShouldReturnEmptyRequest_WhenRowsIsEmpty()
        {
            // Arrange
            var rows = new List<OrganisationDataRow>();

            // Act
            var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SubsidiaryOrganisationDetails);
            Assert.AreEqual(0, result.SubsidiaryOrganisationDetails.Count);
        }

        [TestMethod]
        public void CreateRequest_ShouldGroupByOrganisationReference_AndCreateSubsidiaryDetails()
        {
            // Arrange
            var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1" },
            new() { DefraId = "ORG1", SubsidiaryId = "SUB2" },
            new() { DefraId = "ORG2", SubsidiaryId = "SUB3" },
        };

            // Act
            var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.SubsidiaryOrganisationDetails.Count);

            var org1 = result.SubsidiaryOrganisationDetails.FirstOrDefault(o => o.OrganisationReference == "ORG1");
            Assert.IsNotNull(org1);
            Assert.AreEqual(2, org1.SubsidiaryDetails.Count);
            Assert.IsTrue(org1.SubsidiaryDetails.Any(sub => sub.ReferenceNumber == "SUB1"));
            Assert.IsTrue(org1.SubsidiaryDetails.Any(sub => sub.ReferenceNumber == "SUB2"));

            var org2 = result.SubsidiaryOrganisationDetails.FirstOrDefault(o => o.OrganisationReference == "ORG2");
            Assert.IsNotNull(org2);
            Assert.AreEqual(1, org2.SubsidiaryDetails.Count);
            Assert.IsTrue(org2.SubsidiaryDetails.Any(sub => sub.ReferenceNumber == "SUB3"));
        }

        [TestMethod]
        public void CreateRequest_ShouldExcludeRowsWithEmptySubsidiaryId()
        {
            // Arrange
            var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1" },
            new() { DefraId = "ORG1", SubsidiaryId = string.Empty },
            new() { DefraId = "ORG2", SubsidiaryId = "SUB2" },
        };

            // Act
            var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.SubsidiaryOrganisationDetails.Count);

            var org1 = result.SubsidiaryOrganisationDetails.FirstOrDefault(o => o.OrganisationReference == "ORG1");
            Assert.IsNotNull(org1);
            Assert.AreEqual(1, org1.SubsidiaryDetails.Count);
            Assert.IsTrue(org1.SubsidiaryDetails.Any(sub => sub.ReferenceNumber == "SUB1"));

            var org2 = result.SubsidiaryOrganisationDetails.FirstOrDefault(o => o.OrganisationReference == "ORG2");
            Assert.IsNotNull(org2);
            Assert.AreEqual(1, org2.SubsidiaryDetails.Count);
            Assert.IsTrue(org2.SubsidiaryDetails.Any(sub => sub.ReferenceNumber == "SUB2"));
        }

        [TestMethod]
        public void CreateRequest_WhenSingleSubIsEmptyRecord_ThenReturnCorrectRequest()
        {
            // Arrange
            var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = string.Empty },
        };

            // Act
            var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.SubsidiaryOrganisationDetails.Count);
        }

        [TestMethod]
        public void CreateRequest_ShouldHandleMultipleOrganisations()
        {
            // Arrange
            var rows = new List<OrganisationDataRow>
        {
            new() { DefraId = "ORG1", SubsidiaryId = "SUB1" },
            new() { DefraId = "ORG2", SubsidiaryId = "SUB2" },
            new() { DefraId = "ORG3", SubsidiaryId = "SUB3" },
        };

            // Act
            var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.SubsidiaryOrganisationDetails.Count);

            Assert.IsTrue(result.SubsidiaryOrganisationDetails.Any(org => org.OrganisationReference == "ORG1"));
            Assert.IsTrue(result.SubsidiaryOrganisationDetails.Any(org => org.OrganisationReference == "ORG2"));
            Assert.IsTrue(result.SubsidiaryOrganisationDetails.Any(org => org.OrganisationReference == "ORG3"));
        }
    }
}
