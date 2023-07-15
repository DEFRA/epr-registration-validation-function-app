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
        const int expectedRegistration = 4;

        // Act
        var actualFileUpload = (int)EventType.FileUpload;
        var actualCheckSplitter = (int)EventType.CheckSplitter;
        var actualProducerValidation = (int)EventType.ProducerValidation;
        var actualRegistration = (int)EventType.Registration;

        // Assert
        actualFileUpload.Should().Be(expectedFileUpload);
        actualCheckSplitter.Should().Be(expectedCheckSplitter);
        actualProducerValidation.Should().Be(expectedProducerValidation);
        actualRegistration.Should().Be(expectedRegistration);
    }
}