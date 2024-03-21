namespace EPR.RegistrationValidation.Application.Handlers;

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using Data.Config;
using Microsoft.Extensions.Options;

[ExcludeFromCodeCoverage(Justification = "Dependency on Azure to acquire token, will be assured via end to end testing.")]
public class CompanyDetailsApiAuthorisationHandler : DelegatingHandler
{
    private const string BearerScheme = "Bearer";
    private readonly TokenRequestContext _tokenRequestContext;
    private readonly DefaultAzureCredential? _credentials;

    public CompanyDetailsApiAuthorisationHandler(IOptions<CompanyDetailsApiConfig> options)
    {
        if (string.IsNullOrEmpty(options.Value.ClientId))
        {
            return;
        }

        _tokenRequestContext = new TokenRequestContext(new[] { options.Value.ClientId });
        _credentials = new DefaultAzureCredential();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_credentials != null)
        {
            var tokenResult = await _credentials.GetTokenAsync(_tokenRequestContext, cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, tokenResult.Token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}