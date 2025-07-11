using EPR.RegistrationValidation.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.RegistrationValidation.Functions
{
    using System.Net.Http.Headers;
    using Application.Clients;
    using Application.Extensions;
    using Application.Handlers;
    using Data.Config;
    using EPR.RegistrationValidation.Application.Services;
    using Extensions;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            services.AddLogging();

            services.AddFeatureManagement();
            services.AddConfig();
            services.AddAzureClients();
            services.AddApplication();
            services.AddApplicationInsightsTelemetry();
            services.AddHttpClient<ISubmissionApiClient, SubmissionApiClient>((sp, c) =>
            {
                var submissionApiConfig = sp.GetRequiredService<IOptions<SubmissionApiConfig>>().Value;
                c.BaseAddress = new Uri(submissionApiConfig.BaseUrl);
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            services.AddHttpClient<ICompanyDetailsApiClient, CompanyDetailsApiClient>((sp, c) =>
            {
                var companyDetailsApiConfig = sp.GetRequiredService<IOptions<CompanyDetailsApiConfig>>().Value;
                c.BaseAddress = new Uri(companyDetailsApiConfig.BaseUrl);
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                c.Timeout = TimeSpan.FromSeconds(companyDetailsApiConfig.Timeout);
            })
            .AddHttpMessageHandler<CompanyDetailsApiAuthorisationHandler>();

            services.AddScoped<ApiClients>(sp => new ApiClients
            {
                CompanyDetailsApiClient = sp.GetRequiredService<ICompanyDetailsApiClient>(),
                SubmissionApiClient = sp.GetRequiredService<ISubmissionApiClient>(),
            });
        }
    }
}