namespace EPR.RegistrationValidation.UnitTests.Clients;

using System.Net;
using Application.Clients;
using Data.Config;
using EPR.RegistrationValidation.Application.Exceptions;
using EPR.RegistrationValidation.Data.Models.CompanyDetailsApi;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

[TestClass]
public class CompanyDetailsApiClientTests
{
    private Mock<ILogger<CompanyDetailsApiClient>> _loggerMock = new();
    private CompanyDetailsApiConfig? _config;

    [TestInitialize]
    public void Setup()
    {
        _config = new CompanyDetailsApiConfig { BaseUrl = "https://www.testurl.com" };
        _loggerMock = new Mock<ILogger<CompanyDetailsApiClient>>();
    }

    [TestMethod]
    public async Task TestGetCompanyDetails_WhenSendAsyncIsSuccessful_DoesNotThrowError()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var companyDetails = new CompanyDetailsDataResult();
        var content = JsonConvert.SerializeObject(companyDetails);
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
            BaseAddress = new Uri(_config!.BaseUrl),
        };
        var sut = new CompanyDetailsApiClient(httpClient, _loggerMock.Object);

        // Act
        try
        {
            var response = await sut.GetCompanyDetails("100001");
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
    public async Task TestGetCompanyDetails_WhenSendAsyncNotSuccessful_ThrowsError(HttpStatusCode statusCode)
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
            BaseAddress = new Uri(_config!.BaseUrl),
        };
        var sut = new CompanyDetailsApiClient(httpClient, _loggerMock.Object);

        // Act
        Func<Task> act = () => sut.GetCompanyDetails("100001");

        // Assert
        await act.Should().ThrowAsync<CompanyDetailsApiClientException>();
    }

    [TestMethod]
    public async Task TestGetCompanyDetails_WhenParametersAreValid_BuildsCorrectHTTPRequestMessage()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var organisation = new CompanyDetailsDataItem
        {
            ReferenceNumber = "123456",
            CompaniesHouseNumber = "X1234567",
        };
        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(organisation);
        var companyDetails = new CompanyDetailsDataResult { Organisations = companyDetailsOrganisations };
        var content = JsonConvert.SerializeObject(companyDetails);
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
            BaseAddress = new Uri(_config!.BaseUrl),
        };
        var sut = new CompanyDetailsApiClient(httpClient, _loggerMock.Object);

        // Act
        var responseContent = await sut.GetCompanyDetails("123456");

        // Assert
        var responseAsString = JsonConvert.SerializeObject(responseContent);
        var companyDetailsAsString = JsonConvert.SerializeObject(companyDetails);
        responseAsString.Should().Be(companyDetailsAsString);
    }

    [TestMethod]
    public async Task TestGetComplianceSchemeMembers_WhenSendAsyncIsSuccessful_DoesNotThrowError()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var companyDetails = new CompanyDetailsDataResult();
        var content = JsonConvert.SerializeObject(companyDetails);
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
            BaseAddress = new Uri(_config!.BaseUrl),
        };
        var sut = new CompanyDetailsApiClient(httpClient, _loggerMock.Object);

        // Act
        try
        {
            var response = await sut.GetComplianceSchemeMembers("100001", "13202f0d-bde8-422c-974a-f1dec1b32fff");
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
    public async Task TestGetComplianceSchemeMembers_WhenSendAsyncNotSuccessful_ThrowsError(HttpStatusCode statusCode)
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
            BaseAddress = new Uri(_config!.BaseUrl),
        };
        var sut = new CompanyDetailsApiClient(httpClient, _loggerMock.Object);

        // Act
        Func<Task> act = () => sut.GetComplianceSchemeMembers("100001", "13202f0d-bde8-422c-974a-f1dec1b32fff");

        // Assert
        await act.Should().ThrowAsync<CompanyDetailsApiClientException>();
    }

    [TestMethod]
    public async Task TestGetComplianceSchemeMembers_WhenParametersAreValid_BuildsCorrectHTTPRequestMessage()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(new CompanyDetailsDataItem
        {
            ReferenceNumber = "123456",
            CompaniesHouseNumber = "X1234567",
        });
        companyDetailsOrganisations.Add(new CompanyDetailsDataItem
        {
            ReferenceNumber = "112233",
            CompaniesHouseNumber = "X112233",
        });
        var companyDetails = new CompanyDetailsDataResult { Organisations = companyDetailsOrganisations };
        var content = JsonConvert.SerializeObject(companyDetails);
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
            BaseAddress = new Uri(_config!.BaseUrl),
        };
        var sut = new CompanyDetailsApiClient(httpClient, _loggerMock.Object);

        // Act
        var responseContent = await sut.GetComplianceSchemeMembers("123456", "13202f0d-bde8-422c-974a-f1dec1b32fff");

        // Assert
        var responseAsString = JsonConvert.SerializeObject(responseContent);
        var companyDetailsAsString = JsonConvert.SerializeObject(companyDetails);
        responseContent.Organisations.Count().Should().BeGreaterThan(1);
        responseAsString.Should().Be(companyDetailsAsString);
    }

    [TestMethod]
    public async Task TestGetRemainingProducerDetails_WhenSendAsyncIsSuccessful_DoesNotThrowError()
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
            BaseAddress = new Uri(_config!.BaseUrl),
        };
        var sut = new CompanyDetailsApiClient(httpClient, _loggerMock.Object);
        var referenceNumberList = new List<string>();
        referenceNumberList.Add("11000011");
        referenceNumberList.Add("22000022");
        referenceNumberList.Add("33000033");

        // Act
        try
        {
            var response = await sut.GetRemainingProducerDetails(referenceNumberList);
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
    public async Task TestGetRemainingProducerDetails_WhenSendAsyncNotSuccessful_ThrowsError(HttpStatusCode statusCode)
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
            BaseAddress = new Uri(_config!.BaseUrl),
        };
        var sut = new CompanyDetailsApiClient(httpClient, _loggerMock.Object);
        var referenceNumberList = new List<string>();

        // Act
        Func<Task> act = () => sut.GetRemainingProducerDetails(referenceNumberList);

        // Assert
        await act.Should().ThrowAsync<CompanyDetailsApiClientException>();
    }

    [TestMethod]
    public async Task TestGetRemainingProducerDetails_WhenParametersAreValid_BuildsCorrectHTTPRequestMessage()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var companyDetailsOrganisations = new List<CompanyDetailsDataItem>();
        companyDetailsOrganisations.Add(new CompanyDetailsDataItem
        {
            ReferenceNumber = "123456",
            CompaniesHouseNumber = "X1234567",
        });
        companyDetailsOrganisations.Add(new CompanyDetailsDataItem
        {
            ReferenceNumber = "112233",
            CompaniesHouseNumber = "X112233",
        });
        var companyDetails = new CompanyDetailsDataResult { Organisations = companyDetailsOrganisations };
        var content = JsonConvert.SerializeObject(companyDetails);
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
            BaseAddress = new Uri(_config!.BaseUrl),
        };
        var sut = new CompanyDetailsApiClient(httpClient, _loggerMock.Object);
        var referenceNumberList = new List<string>();
        referenceNumberList.Add("11000011");
        referenceNumberList.Add("22000022");
        referenceNumberList.Add("33000033");

        // Act
        var responseContent = await sut.GetRemainingProducerDetails(referenceNumberList);

        // Assert
        var responseAsString = JsonConvert.SerializeObject(responseContent);
        var companyDetailsAsString = JsonConvert.SerializeObject(companyDetails);
        responseContent.Organisations.Count().Should().BeGreaterThan(1);
        responseAsString.Should().Be(companyDetailsAsString);
    }
}