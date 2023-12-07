namespace EPR.RegistrationValidation.UnitTests.Providers;

using Application.Exceptions;
using Application.Providers;
using Data.Enums;
using Data.Models.QueueMessages;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelpers;

[TestClass]
public class DequeueProviderTests
{
    private DequeueProvider _sut = new DequeueProvider();

    [TestInitialize]
    public void SetUp()
    {
    }

    [TestMethod]
    public void TestGetMessageFromJson_WhenMessageIsValid_ReturnsResponse()
    {
        // Arrange
        var submissionId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var organisationId = Guid.NewGuid().ToString();
        var submissionSubType = SubmissionSubType.CompanyDetails;
        var userType = UserType.Producer.ToString();
        var blobName = "BN";
        var message = QueueMessageTestHelper.GenerateMessage(blobName, submissionId, submissionSubType.ToString(), userId, organisationId, userType);

        // Act
        var response = _sut.GetMessageFromJson<BlobQueueMessage>(message);

        // Assert
        response.OrganisationId.Should().Be(organisationId);
        response.UserId.Should().Be(userId);
        response.SubmissionId.Should().Be(submissionId);
        response.SubmissionSubType.Should().Be(submissionSubType.ToString());
        response.BlobName.Should().Be(blobName);
        response.UserType.Should().Be(userType);
    }

    [TestMethod]
    public void TestGetMessageFromJson_WhenMessageIsInvalid_ThrowsError()
    {
        // Arrange
        var message = "invalid";

        // Act
        Action act = () => _sut.GetMessageFromJson<BlobQueueMessage>(message);

        // Assert
        act.Should().Throw<DeserializeQueueException>();
    }
}