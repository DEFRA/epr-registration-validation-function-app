namespace EPR.RegistrationValidation.UnitTests.Clients;

using System.Net;
using Application.Clients;
using Data.Config;
using EPR.RegistrationValidation.Application.Exceptions;
using EPR.RegistrationValidation.Data.Models.CompanyDetailsApi;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

[TestClass]
public class CompanyDetailsApiClientTests
{
    private CompanyDetailsApiConfig? _config;

    [TestInitialize]
    public void Setup()
    {
        _config = new CompanyDetailsApiConfig
        {
            BaseUrl = "https://www.testurl.com",
            ClientId = "test-client-id",
            Timeout = 5,
        };
    }

    [TestMethod]
    public async Task GetCompanyDetails_WhenSendAsyncIsSuccessful_DoesNotThrowError()
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
            BaseAddress = new Uri(_config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_config.Timeout),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

        // Act
        var response = await sut.GetCompanyDetails("100001");

        // Assert
        response.Should().NotBeNull("Exception not expected");
    }

    [DataRow(HttpStatusCode.Conflict)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [TestMethod]
    public async Task GetCompanyDetails_WhenSendAsyncNotSuccessful_ThrowsError(HttpStatusCode statusCode)
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
            Timeout = TimeSpan.FromSeconds(_config.Timeout),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

        // Act
        Func<Task> act = () => sut.GetCompanyDetails("100001");

        // Assert
        await act.Should().ThrowAsync<CompanyDetailsApiClientException>();
    }

    [TestMethod]
    public async Task GetCompanyDetails_WhenSendAsyncNotFound_ReturnsNull()
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
                StatusCode = HttpStatusCode.NotFound,
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

        // Act
        var result = await sut.GetCompanyDetails("100001");

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetCompanyDetails_WhenParametersAreValid_BuildsCorrectHTTPRequestMessage()
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
            BaseAddress = new Uri(_config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_config.Timeout),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

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
            BaseAddress = new Uri(_config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_config.Timeout),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

        // Act
        var response = await sut.GetComplianceSchemeMembers("100001", "13202f0d-bde8-422c-974a-f1dec1b32fff");

        // Assert
        response.Should().NotBeNull("Exception not expected");
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
            BaseAddress = new Uri(_config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_config.Timeout),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

        // Act
        Func<Task> act = () => sut.GetComplianceSchemeMembers("100001", "13202f0d-bde8-422c-974a-f1dec1b32fff");

        // Assert
        await act.Should().ThrowAsync<CompanyDetailsApiClientException>();
    }

    [TestMethod]
    public async Task TestGetComplianceSchemeMembers_WhenSendAsyncNotFound_ReturnsNull()
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
                StatusCode = HttpStatusCode.NotFound,
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

        // Act
        var result = await sut.GetComplianceSchemeMembers("100001", "13202f0d-bde8-422c-974a-f1dec1b32fff");

        // Assert
        result.Should().BeNull();
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
            BaseAddress = new Uri(_config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_config.Timeout),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

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
            BaseAddress = new Uri(_config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_config.Timeout),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);
        var referenceNumberList = new List<string>();
        referenceNumberList.Add("11000011");
        referenceNumberList.Add("22000022");
        referenceNumberList.Add("33000033");

        // Act
        var response = await sut.GetRemainingProducerDetails(referenceNumberList, string.Empty);

        // Assert
        response.Should().NotBeNull("Exception not expected");
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
            BaseAddress = new Uri(_config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_config.Timeout),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);
        var referenceNumberList = new List<string>();

        // Act
        Func<Task> act = () => sut.GetRemainingProducerDetails(referenceNumberList, string.Empty);

        // Assert
        await act.Should().ThrowAsync<CompanyDetailsApiClientException>();
    }

    [TestMethod]
    public async Task TestGetRemainingProducerDetails_WhenSendAsyncNotFound_ReturnsNull()
    {
        // Arrange
        var referenceNumberList = new List<string>();

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
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

        // Act
        var result = await sut.GetRemainingProducerDetails(referenceNumberList, string.Empty);

        // Assert
        result.Should().BeNull();
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
            BaseAddress = new Uri(_config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_config.Timeout),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);
        var referenceNumberList = new List<string>();
        referenceNumberList.Add("11000011");
        referenceNumberList.Add("22000022");
        referenceNumberList.Add("33000033");

        // Act
        var responseContent = await sut.GetRemainingProducerDetails(referenceNumberList, string.Empty);

        // Assert
        var responseAsString = JsonConvert.SerializeObject(responseContent);
        var companyDetailsAsString = JsonConvert.SerializeObject(companyDetails);
        responseContent.Organisations.Count().Should().BeGreaterThan(1);
        responseAsString.Should().Be(companyDetailsAsString);
    }

    [TestMethod]
    public async Task GetCompanyDetailsByProducer_WhenSendAsyncIsSuccessful_DoesNotThrowError()
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
            BaseAddress = new Uri(_config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_config.Timeout),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

        // Act
        var response = await sut.GetCompanyDetailsByProducer("9649AF7C-A3EE-44BE-BCD5-ECCD92428125");

        // Assert
        response.Should().NotBeNull("Exception not expected");
    }

    [DataRow(HttpStatusCode.Conflict)]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.BadGateway)]
    [DataRow(HttpStatusCode.Unauthorized)]
    [TestMethod]
    public async Task GetCompanyDetailsByProducer_WhenSendAsyncNotSuccessful_ThrowsError(HttpStatusCode statusCode)
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
            Timeout = TimeSpan.FromSeconds(_config.Timeout),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

        // Act
        Func<Task> act = () => sut.GetCompanyDetailsByProducer("9649AF7C-A3EE-44BE-BCD5-ECCD92428125");

        // Assert
        await act.Should().ThrowAsync<CompanyDetailsApiClientException>();
    }

    [TestMethod]
    public async Task GetCompanyDetailsByProducer_WhenSendAsyncNotFound_ReturnsNull()
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
                StatusCode = HttpStatusCode.NotFound,
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

        // Act
        var result = await sut.GetCompanyDetailsByProducer("9649AF7C-A3EE-44BE-BCD5-ECCD92428125");

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetCompanyDetailsByProducer_WhenParametersAreValid_BuildsCorrectHTTPRequestMessage()
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
            BaseAddress = new Uri(_config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_config.Timeout),
        };
        var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

        // Act
        var responseContent = await sut.GetCompanyDetailsByProducer("9649AF7C-A3EE-44BE-BCD5-ECCD92428125");

        // Assert
        var responseAsString = JsonConvert.SerializeObject(responseContent);
        var companyDetailsAsString = JsonConvert.SerializeObject(companyDetails);
        responseAsString.Should().Be(companyDetailsAsString);
    }
}