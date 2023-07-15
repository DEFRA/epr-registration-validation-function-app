using EPR.RegistrationValidation.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.RegistrationValidation.Functions
{
    using System.Net.Http.Headers;
    using Application.Clients;
    using Application.Extensions;
    using Data.Config;
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
        }
    }
}