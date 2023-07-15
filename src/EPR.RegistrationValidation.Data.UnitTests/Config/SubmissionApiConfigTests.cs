namespace EPR.RegistrationValidation.Data.UnitTests.Config;

using System.ComponentModel.DataAnnotations;
using Data.Config;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class SubmissionApiConfigTests
{
    [TestMethod]
    public void SubmissionApiConfig_BaseUrlIsRequired_Success()
    {
        // Arrange
        var config = new SubmissionApiConfig
        {
            BaseUrl = "https://example.com",
            Version = "v1",
            SubmissionEndpoint = "/submit",
            SubmissionEventEndpoint = "/submit/event",
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [TestMethod]
    public void SubmissionApiConfig_BaseUrlIsRequired_Failure()
    {
        // Arrange
        var config = new SubmissionApiConfig
        {
            BaseUrl = null,
            Version = "v1",
            SubmissionEndpoint = "/submit",
            SubmissionEventEndpoint = "/submit/event",
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(config.BaseUrl)));
    }

    [TestMethod]
    public void TestSubmissionApiConfig_VersionIsRequired_Success()
    {
        // Arrange
        var config = new SubmissionApiConfig
        {
            BaseUrl = "https://example.com",
            Version = "v1",
            SubmissionEndpoint = "/submit",
            SubmissionEventEndpoint = "/submit/event",
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [TestMethod]
    public void TestSubmissionApiConfig_VersionIsRequired_Failure()
    {
        // Arrange
        var config = new SubmissionApiConfig
        {
            BaseUrl = "https://example.com",
            Version = null,
            SubmissionEndpoint = "/submit",
            SubmissionEventEndpoint = "/submit/event",
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(config.Version)));
    }

    [TestMethod]
    public void TestSubmissionApiConfig_SubmissionEndpointIsRequired_Success()
    {
        // Arrange
        var config = new SubmissionApiConfig
        {
            BaseUrl = "https://example.com",
            Version = "v1",
            SubmissionEndpoint = "/submit",
            SubmissionEventEndpoint = "/submit/event",
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [TestMethod]
    public void TestSubmissionApiConfig_SubmissionEndpointIsRequired_Failure()
    {
        // Arrange
        var config = new SubmissionApiConfig
        {
            BaseUrl = "https://example.com",
            Version = "v1",
            SubmissionEndpoint = null,
            SubmissionEventEndpoint = "/submit/event",
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(config.SubmissionEndpoint)));
    }

    [TestMethod]
    public void TestSubmissionApiConfig_SubmissionEventEndpoint_Success()
    {
        // Arrange
        var config = new SubmissionApiConfig
        {
            BaseUrl = "https://example.com",
            Version = "v1",
            SubmissionEndpoint = "/submit",
            SubmissionEventEndpoint = "/submit/event",
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [TestMethod]
    public void TestSubmissionApiConfig_SubmissionEventEndpoint_Failure()
    {
        // Arrange
        var config = new SubmissionApiConfig
        {
            BaseUrl = "https://example.com",
            Version = "v1",
            SubmissionEndpoint = "/submit",
            SubmissionEventEndpoint = null,
        };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(config.SubmissionEventEndpoint)));
    }
}