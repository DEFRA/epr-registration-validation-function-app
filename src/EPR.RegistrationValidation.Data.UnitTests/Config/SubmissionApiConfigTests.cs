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
        var config = new SubmissionApiConfig { BaseUrl = "https://example.com" };

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
        var config = new SubmissionApiConfig { BaseUrl = null };

        // Act
        var context = new ValidationContext(config, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(config.BaseUrl)));
    }
}