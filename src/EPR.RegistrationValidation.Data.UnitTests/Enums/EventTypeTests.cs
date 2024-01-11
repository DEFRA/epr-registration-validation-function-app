namespace EPR.RegistrationValidation.Data.UnitTests.Config;

using Data.Enums;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class EventTypeTests
{
    [TestMethod]
    public void TestEventType_EnumValues_AreDefinedCorrectly()
    {
        // Arrange
        const int expectedFileUpload = 1;
        const int expectedCheckSplitter = 2;
        const int expectedProducerValidation = 3;
        const int expectedOrganisationRegistration = 4;
        const int expectedBrandRegistration = 8;
        const int expectedPartnerRegistration = 9;

        // Act
        var actualFileUpload = (int)EventType.FileUpload;
        var actualCheckSplitter = (int)EventType.CheckSplitter;
        var actualProducerValidation = (int)EventType.ProducerValidation;
        var actualRegistration = (int)EventType.Registration;
        var actualBrandRegistration = (int)EventType.BrandValidation;
        var actualPartnerRegistration = (int)EventType.PartnerValidation;

        // Assert
        actualFileUpload.Should().Be(expectedFileUpload);
        actualCheckSplitter.Should().Be(expectedCheckSplitter);
        actualProducerValidation.Should().Be(expectedProducerValidation);
        actualRegistration.Should().Be(expectedOrganisationRegistration);
        actualBrandRegistration.Should().Be(expectedBrandRegistration);
        actualPartnerRegistration.Should().Be(expectedPartnerRegistration);
    }
}