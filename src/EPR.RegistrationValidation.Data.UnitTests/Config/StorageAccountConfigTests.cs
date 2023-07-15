namespace EPR.RegistrationValidation.Data.UnitTests.Config;

using System.ComponentModel.DataAnnotations;
using Data.Config;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class StorageAccountConfigTests
{
    [TestMethod]
    public void TestStorageAccountConfig_ConnectionStringIsRequired_Success()
    {
        // Arrange
        var config = new StorageAccountConfig
        {
            ConnectionString = "myconnectionstring",
            BlobContainerName = "mycontainer",
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [TestMethod]
    public void TestStorageAccountConfig_ConnectionStringIsRequired_Failure()
    {
        // Arrange
        var config = new StorageAccountConfig
        {
            ConnectionString = null,
            BlobContainerName = "mycontainer",
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
    public void TestStorageAccountConfig_BlobContainerNameIsRequired_Success()
    {
        // Arrange
        var config = new StorageAccountConfig
        {
            ConnectionString = "myconnectionstring",
            BlobContainerName = "mycontainer",
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [TestMethod]
    public void TestStorageAccountConfig_BlobContainerNameIsRequired_Failure()
    {
        // Arrange
        var config = new StorageAccountConfig
        {
            ConnectionString = "myconnectionstring",
            BlobContainerName = null,
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(config.BlobContainerName)));
    }
}