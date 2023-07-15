namespace EPR.RegistrationValidation.Data.UnitTests.Config;

using System.ComponentModel.DataAnnotations;
using Data.Config;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ServiceBusConfigTests
{
    [TestMethod]
    public void TestServiceBusConfig_ConnectionStringIsRequired_Success()
    {
        // Arrange
        var config = new ServiceBusConfig
        {
            ConnectionString = "myconnectionstring",
            UploadQueueName = "myqueue",
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [TestMethod]
    public void TestServiceBusConfig_ConnectionStringIsRequired_Failure()
    {
        // Arrange
        var config = new ServiceBusConfig
        {
            ConnectionString = null,
            UploadQueueName = "myqueue",
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(config.ConnectionString)));
    }

    [TestMethod]
    public void TestServiceBusConfig_UploadQueueNameIsRequired_Success()
    {
        // Arrange
        var config = new ServiceBusConfig
        {
            ConnectionString = "myconnectionstring",
            UploadQueueName = "myqueue",
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [TestMethod]
    public void TestServiceBusConfig_UploadQueueNameIsRequired_Failure()
    {
        // Arrange
        var config = new ServiceBusConfig
        {
            ConnectionString = "myconnectionstring",
            UploadQueueName = null,
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(config.UploadQueueName)));
    }
}