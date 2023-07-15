namespace EPR.RegistrationValidation.Data.UnitTests.Config;

using Data.Enums;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class UserTypeTests
{
    [TestMethod]
    public void TestsUserType_EnumValues_AreDefinedCorrectly()
    {
        // Arrange
        const UserType expectedProducer = UserType.Producer;
        const UserType expectedComplianceScheme = UserType.ComplianceScheme;
        const UserType expectedRegulator = UserType.Regulator;

        // Act
        var actualProducer = UserType.Producer;
        var actualComplianceScheme = UserType.ComplianceScheme;
        var actualRegulator = UserType.Regulator;

        // Assert
        actualProducer.Should().Be(expectedProducer);
        actualComplianceScheme.Should().Be(expectedComplianceScheme);
        actualRegulator.Should().Be(expectedRegulator);
    }
}