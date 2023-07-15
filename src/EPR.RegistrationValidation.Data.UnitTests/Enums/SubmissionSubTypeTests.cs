namespace EPR.RegistrationValidation.Data.UnitTests.Config;

using Data.Enums;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class SubmissionSubTypeTests
{
    [TestMethod]
    public void TestSubmissionSubType_EnumValues_AreDefinedCorrectly()
    {
        // Arrange
        const SubmissionSubType expectedCompanyDetails = SubmissionSubType.CompanyDetails;
        const SubmissionSubType expectedBrands = SubmissionSubType.Brands;
        const SubmissionSubType expectedPartnerships = SubmissionSubType.Partnerships;

        // Act
        var actualCompanyDetails = SubmissionSubType.CompanyDetails;
        var actualBrands = SubmissionSubType.Brands;
        var actualPartnerships = SubmissionSubType.Partnerships;

        // Assert
        actualCompanyDetails.Should().Be(expectedCompanyDetails);
        actualBrands.Should().Be(expectedBrands);
        actualPartnerships.Should().Be(expectedPartnerships);
    }
}