namespace EPR.RegistrationValidation.Application.Clients;

using System.Net;
using System.Text;
using Data.Config;
using Data.Models.SubmissionApi;
using EPR.RegistrationValidation.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class SubmissionApiClient : ISubmissionApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SubmissionApiConfig _config;
    private readonly ILogger<SubmissionApiClient> _logger;

    public SubmissionApiClient(HttpClient httpClient, IOptions<SubmissionApiConfig> options, ILogger<SubmissionApiClient> logger)
    {
        _httpClient = httpClient;
        _config = options.Value;
        _logger = logger;
    }

    public async Task<OrganisationFileDetailsResponse> GetOrganisationFileDetails(string submissionId, string brandOrPartnerBlobName)
    {
        try
        {
            var uriString = $"{_config.BaseUrl}/v1.0/submissions/{submissionId}/organisation-details?blobName={brandOrPartnerBlobName}";
            var response = await _httpClient.GetAsync(uriString);

            if (HttpStatusCode.NotFound.Equals(response.StatusCode))
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<OrganisationFileDetailsResponse>(await response.Content.ReadAsStringAsync());
        }
        catch (HttpRequestException exception)
        {
            const string message = "A success status code was not received when requesting Organisation details";
            _logger.LogError(exception, message);
            throw new SubmissionApiClientException(message, exception);
        }
    }

    public async Task SendEventRegistrationMessage(
        string orgId,
        string userId,
        string submissionId,
        string userType,
        ValidationEvent validationEvent)
    {
        var request = BuildRequestMessage(orgId, userId, submissionId, userType, validationEvent);
        var res = await _httpClient.SendAsync(request);
        res.EnsureSuccessStatusCode();
    }

    public HttpRequestMessage BuildRequestMessage(
        string orgId,
        string userId,
        string submissionId,
        string userType,
        ValidationEvent validationEvent)
    {
        var uriString = $"{_config.BaseUrl}/v1/submissions/{submissionId}/events";
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(uriString),
            Headers =
            {
                {
                    "organisationId", orgId
                },
                {
                    "userId", userId
                },
                {
                    "userType", userType
                },
            },
            Content = new StringContent(
                JsonConvert.SerializeObject(
                    validationEvent,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    }),
                Encoding.UTF8,
                "application/json"),
        };

        return httpRequestMessage;
    }
}