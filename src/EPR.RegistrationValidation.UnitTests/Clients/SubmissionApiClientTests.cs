namespace EPR.RegistrationValidation.UnitTests.Clients;

using System.Net;
using System.Text;
using Application.Clients;
using Data.Config;
using Data.Enums;
using Data.Models;
using FluentAssertions;
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
        _config = new SubmissionApiConfig
        {
            BaseUrl = "https://www.testurl.com",
            Version = "1",
            SubmissionEndpoint = "submission",
            SubmissionEventEndpoint = "event",
        };
        _submissionApiOptionsMock.Setup(x => x.Value).Returns(_config);
    }

    [TestMethod]
    [DataRow(RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredPackagingActivityForBrands.Secondary)]
    public async Task TestSendEventRegistrationMessage_WhenSendAsyncIsSuccessful_DoesNotThrowError(RequiredPackagingActivityForBrands packagingActivity)
    {
        // ARRANGE
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
            BaseAddress = new Uri(_config!.BaseUrl),
        };
        var sut = new SubmissionApiClient(httpClient, _submissionApiOptionsMock.Object);
        var csvDataRow = CSVRowTestHelper.GenerateCSVDataRowTestHelper(
            RequiredOrganisationTypeCodeForPartners.PAR.ToString(),
            packagingActivity.ToString());
        var csvDataRows = new List<CsvDataRow>
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

        // ACT
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
            // ASSERT
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
        // ARRANGE
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
            BaseAddress = new Uri(_config!.BaseUrl),
        };
        var sut = new SubmissionApiClient(httpClient, _submissionApiOptionsMock.Object);
        var csvDataRow = CSVRowTestHelper.GenerateCSVDataRowTestHelper(
            RequiredOrganisationTypeCodeForPartners.PAR.ToString(),
            RequiredPackagingActivityForBrands.Primary.ToString());
        var csvDataRows = new List<CsvDataRow>
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

        // ACT
        Func<Task> act = () => sut.SendEventRegistrationMessage(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            userType,
            regEvent);

        // ASSERT
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [TestMethod]
    [DataRow(RequiredPackagingActivityForBrands.Primary)]
    [DataRow(RequiredPackagingActivityForBrands.Secondary)]
    public async Task TestBuildRequestMessage_WhenParametersAreValid_BuildsCorrectHTTPRequestMessage(RequiredPackagingActivityForBrands packagingActivity)
    {
        // ARRANGE
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
            BaseAddress = new Uri(_config!.BaseUrl),
        };
        var sut = new SubmissionApiClient(httpClient, _submissionApiOptionsMock.Object);
        var csvDataRow = CSVRowTestHelper.GenerateCSVDataRowTestHelper(
            RequiredOrganisationTypeCodeForPartners.PAR.ToString(),
            packagingActivity.ToString());
        var csvDataRows = new List<CsvDataRow>
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

        // ACT
        var response = sut.BuildRequestMessage(
            orgId.ToString(),
            userId.ToString(),
            Guid.NewGuid().ToString(),
            userType,
            regEvent);

        // ASSERT
        response.Method.Should().Be(HttpMethod.Post);
        response.Headers.GetValues("organisationId").Should().BeEquivalentTo(orgId.ToString());
        response.Headers.GetValues("userId").Should().BeEquivalentTo(userId.ToString());
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Be(content);
    }
}