﻿namespace EPR.RegistrationValidation.Application.Clients;

using System.Text;
using Data.Config;
using Data.Models.SubmissionApi;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class SubmissionApiClient : ISubmissionApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SubmissionApiConfig _config;

    public SubmissionApiClient(HttpClient httpClient, IOptions<SubmissionApiConfig> options)
    {
        _httpClient = httpClient;
        _config = options.Value;
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