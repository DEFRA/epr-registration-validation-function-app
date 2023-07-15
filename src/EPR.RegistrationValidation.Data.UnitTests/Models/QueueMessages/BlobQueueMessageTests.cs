namespace EPR.RegistrationValidation.Data.UnitTests.Models.QueueMessages;

using System.ComponentModel.DataAnnotations;
using Data.Enums;
using Data.Models.QueueMessages;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class BlobQueueMessageTests
{
    [TestMethod]
    public void BlobQueueMessage_Validation_ReturnsExpectedResults()
    {
        // Arrange
        var blobQueueMessage = new BlobQueueMessage
        {
            BlobName = "example.txt",
            SubmissionId = "123",
            SubmissionSubType = SubmissionSubType.CompanyDetails.ToString(),
            UserType = UserType.Producer.ToString(),
            UserId = "456",
            OrganisationId = "789",
        };

        // Act
        var validationResults = ValidateModel(blobQueueMessage);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [TestMethod]
    public void BlobQueueMessage_Validation_ReturnsErrorWhenBlobNameMissing()
    {
        // Arrange
        var blobQueueMessage = new BlobQueueMessage
        {
            SubmissionId = "123",
            SubmissionSubType = SubmissionSubType.CompanyDetails.ToString(),
            UserType = UserType.Producer.ToString(),
            UserId = "456",
            OrganisationId = "789",
        };

        // Act
        var validationResults = ValidateModel(blobQueueMessage);

        // Assert
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("The BlobName field is required.");
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationContext = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}