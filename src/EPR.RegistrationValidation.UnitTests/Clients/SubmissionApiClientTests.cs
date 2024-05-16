namespace EPR.RegistrationValidation.UnitTests.Clients;

using System.Net;
using System.Text;
using Application.Clients;
using Data.Config;
using Data.Enums;
using Data.Models;
using EPR.RegistrationValidation.Application.Exceptions;
using EPR.RegistrationValidation.Data.Models.SubmissionApi;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TestHelpers;

[TestClass]
public class SubmissionApiClientTests
{
    private readonly Mock<IOptions<SubmissionApiConfig>> _submissionApiOptionsMock = new();
    private SubmissionApiConfig? _config;

    [TestInitialize]
    public void Setup()
    {
        _config = new SubmissionApiConfig { BaseUrl = "https://www.testurl.com" };
        _submissionApiOptionsMock.Setup(x => x.Value).Returns(_config);
    }

    [TestMethod]
    [DataRow(RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredPackagingActivityForBrands.Secondary)]
    public async Task TestSendEventRegistrationMessage_WhenSendAsyncIsSuccessful_DoesNotThrowError(RequiredPackagingActivityForBrands packagingActivity)
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
            })
            .Verifiable();
        var userType = UserType.Producer.ToString();
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl),
        };
        var sut = new SubmissionApiClient(httpClient, _submissionApiOptionsMock.Object, NullLogger<SubmissionApiClient>.Instance);
        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(
            RequiredOrganisationTypeCodeForPartners.PAR.ToString(),
            packagingActivity.ToString());
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };
        var regEvent = RegistrationEventTestHelper.BuildRegistrationEvent(
            csvDataRows,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            null,
            true,
            null);

        // Act
        try
        {
            await sut.SendEventRegistrationMessage(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                userType,
                regEvent);
        }
        catch (Exception ex)
        {
            // Assert
            Assert.Fail("Expected no exception, but got: " + ex.Message);
        }
    }

    [DataRow(HttpStatusCode.Conflict)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [TestMethod]
    public async Task TestSendEventRegistrationMessage_WhenSendAsyncNotSuccessful_ThrowsError(HttpStatusCode statusCode)
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = statusCode,
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl),
        };
        var sut = new SubmissionApiClient(httpClient, _submissionApiOptionsMock.Object, NullLogger<SubmissionApiClient>.Instance);
        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(
            RequiredOrganisationTypeCodeForPartners.PAR.ToString(),
            RequiredPackagingActivityForBrands.Primary.ToString());
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };
        var userType = UserType.Producer.ToString();
        var regEvent = RegistrationEventTestHelper.BuildRegistrationEvent(
            csvDataRows,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            null,
            true,
            null);

        // Act
        Func<Task> act = () => sut.SendEventRegistrationMessage(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            userType,
            regEvent);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [TestMethod]
    [DataRow(RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredPackagingActivityForBrands.Secondary)]
    public async Task TestBuildRequestMessage_WhenParametersAreValid_BuildsCorrectHTTPRequestMessage(RequiredPackagingActivityForBrands packagingActivity)
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl),
        };
        var sut = new SubmissionApiClient(httpClient, _submissionApiOptionsMock.Object, NullLogger<SubmissionApiClient>.Instance);
        var csvDataRow = CSVRowTestHelper.GenerateOrgCsvDataRow(
            RequiredOrganisationTypeCodeForPartners.PAR.ToString(),
            packagingActivity.ToString());
        var csvDataRows = new List<OrganisationDataRow>
        {
            csvDataRow,
        };

        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var regEvent = RegistrationEventTestHelper.BuildRegistrationEvent(
            csvDataRows,
            Guid.NewGuid().ToString(),
            userId.ToString(),
            orgId.ToString(),
            null,
            true,
            null);
        var content = await new StringContent(
            JsonConvert.SerializeObject(
                regEvent,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                }),
            Encoding.UTF8,
            "application/json").ReadAsStringAsync();
        var userType = UserType.Producer.ToString();

        // Act
        var response = sut.BuildRequestMessage(
            orgId.ToString(),
            userId.ToString(),
            Guid.NewGuid().ToString(),
            userType,
            regEvent);

        // Assert
        response.Method.Should().Be(HttpMethod.Post);
        response.Headers.GetValues("organisationId").Should().BeEquivalentTo(orgId.ToString());
        response.Headers.GetValues("userId").Should().BeEquivalentTo(userId.ToString());
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Be(content);
    }

    [TestMethod]
    public async Task TestGetOrganisationFileDetails_WhenSendAsyncIsSuccessful_DoesNotThrowError()
    {
        // Arrange
        const string submissionId = "fa9d10d7-cd5d-404b-8a3a-cd66b6564824";
        const string brandOrPartnerBlobName = "0455196e-236e-4070-bdd7-0cea56fb3bdc";

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var organisationFileDetails = new OrganisationFileDetailsResponse();
        var content = JsonConvert.SerializeObject(organisationFileDetails);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl),
        };
        var sut = new SubmissionApiClient(httpClient, _submissionApiOptionsMock.Object, NullLogger<SubmissionApiClient>.Instance);

        // Act
        var response = await sut.GetOrganisationFileDetails(submissionId, brandOrPartnerBlobName);

        // Assert
        response.Should().NotBeNull("Exception not expected");
    }

    [DataRow(HttpStatusCode.Conflict)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [TestMethod]
    public async Task TestGetOrganisationFileDetails_WhenSendAsyncNotSuccessful_ThrowsError(HttpStatusCode statusCode)
    {
        // Arrange
        const string submissionId = "fa9d10d7-cd5d-404b-8a3a-cd66b6564824";
        const string brandOrPartnerBlobName = "0455196e-236e-4070-bdd7-0cea56fb3bdc";

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = statusCode,
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl),
        };
        var sut = new SubmissionApiClient(httpClient, _submissionApiOptionsMock.Object, NullLogger<SubmissionApiClient>.Instance);

        // Act
        Func<Task> act = () => sut.GetOrganisationFileDetails(submissionId, brandOrPartnerBlobName);

        // Assert
        await act.Should().ThrowAsync<SubmissionApiClientException>();
    }

    [TestMethod]
    public async Task TestGetOrganisationFileDetails_WhenSendAsyncNotFound_ReturnsNull()
    {
        // Arrange
        const string submissionId = "fa9d10d7-cd5d-404b-8a3a-cd66b6564824";
        const string brandOrPartnerBlobName = "0455196e-236e-4070-bdd7-0cea56fb3bdc";

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NotFound,
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl),
        };
        var sut = new SubmissionApiClient(httpClient, _submissionApiOptionsMock.Object, NullLogger<SubmissionApiClient>.Instance);

        // Act
        var result = await sut.GetOrganisationFileDetails(submissionId, brandOrPartnerBlobName);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task TestGetOrganisationFileDetails_WhenParametersAreValid_ReturnsCorrectResponse()
    {
        // Arrange
        const string submissionId = "fa9d10d7-cd5d-404b-8a3a-cd66b6564824";
        const string brandOrPartnerBlobName = "0455196e-236e-4070-bdd7-0cea56fb3bdc";

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var registrationBlobName = "92fcddf5-f66b-4f0f-8a13-f5a3b38dd48f";
        var organisationFileDetails = new OrganisationFileDetailsResponse { BlobName = registrationBlobName };
        var content = JsonConvert.SerializeObject(organisationFileDetails);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl),
        };
        var sut = new SubmissionApiClient(httpClient, _submissionApiOptionsMock.Object, NullLogger<SubmissionApiClient>.Instance);

        // Act
        var response = await sut.GetOrganisationFileDetails(submissionId, brandOrPartnerBlobName);

        // Assert
        response.Should().NotBeNull();
        response.BlobName.Should().Be(registrationBlobName);
    }
}