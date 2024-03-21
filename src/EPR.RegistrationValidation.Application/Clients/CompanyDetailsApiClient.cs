namespace EPR.RegistrationValidation.Application.Clients;

using System.Net;
using System.Net.Http;
using Data.Models.CompanyDetailsApi;
using Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class CompanyDetailsApiClient : ICompanyDetailsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CompanyDetailsApiClient> _logger;

    public CompanyDetailsApiClient(
        HttpClient httpClient,
        ILogger<CompanyDetailsApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CompanyDetailsDataResult> GetCompanyDetails(string organisationId)
    {
        try
        {
            var uriString = $"api/company-details/{organisationId}";
            var response = await _httpClient.GetAsync(uriString);

            if (HttpStatusCode.NotFound.Equals(response.StatusCode))
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Organisation details received from Company Details Api");
            }

            if (!string.IsNullOrEmpty(await response.Content.ReadAsStringAsync()))
            {
                return JsonConvert.DeserializeObject<CompanyDetailsDataResult>(await response.Content.ReadAsStringAsync());
            }

            return new CompanyDetailsDataResult();
        }
        catch (HttpRequestException exception)
        {
            const string message = "A success status code was not received when requesting company details";
            _logger.LogError(exception, message);
            throw new CompanyDetailsApiClientException(message, exception);
        }
    }

    public async Task<CompanyDetailsDataResult> GetComplianceSchemeMembers(string organisationId, string complianceSchemeId)
    {
        try
        {
            var uriString = $"api/company-details/{organisationId}/compliance-scheme/{complianceSchemeId}";
            var response = await _httpClient.GetAsync(uriString);

            if (HttpStatusCode.NotFound.Equals(response.StatusCode))
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Compliance scheme member list received from Company Details Api");
            }

            if (!string.IsNullOrEmpty(await response.Content.ReadAsStringAsync()))
            {
                return JsonConvert.DeserializeObject<CompanyDetailsDataResult>(await response.Content.ReadAsStringAsync());
            }

            return new CompanyDetailsDataResult();
        }
        catch (HttpRequestException exception)
        {
            const string message = "A success status code was not received when requesting compliance scheme member list";
            _logger.LogError(exception, message);
            throw new CompanyDetailsApiClientException(message, exception);
        }
    }

    public async Task<CompanyDetailsDataResult> GetRemainingProducerDetails(IEnumerable<string> referenceNumbers)
    {
        try
        {
            var uriString = $"api/company-details";

            var json = JsonConvert.SerializeObject(referenceNumbers);
            var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(uriString, httpContent);

            if (HttpStatusCode.NotFound.Equals(response.StatusCode))
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Remaining producer details received from Company Details Api");
            }

            if (!string.IsNullOrEmpty(await response.Content.ReadAsStringAsync()))
            {
                return JsonConvert.DeserializeObject<CompanyDetailsDataResult>(await response.Content.ReadAsStringAsync());
            }

            return new CompanyDetailsDataResult();
        }
        catch (HttpRequestException exception)
        {
            const string message = "A success status code was not received when requesting remaining producer company details";
            _logger.LogError(exception, message);
            throw new CompanyDetailsApiClientException(message, exception);
        }
    }
}