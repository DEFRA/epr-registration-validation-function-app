using EPR.RegistrationValidation.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.FeatureManagement;
using Polly;
using Polly.Retry;
using Polly.Timeout;

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
            })
            .AddResilienceHandler("SubmissionApiResiliencePipeline", BuildResiliencePipeline());

            services.AddHttpClient<ICompanyDetailsApiClient, CompanyDetailsApiClient>((sp, c) =>
            {
                var companyDetailsApiConfig = sp.GetRequiredService<IOptions<CompanyDetailsApiConfig>>().Value;
                c.BaseAddress = new Uri(companyDetailsApiConfig.BaseUrl);
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<CompanyDetailsApiAuthorisationHandler>()
            .AddResilienceHandler("CompanyDetailsResiliencePipeline", BuildResiliencePipeline<CompanyDetailsApiConfig>(o => TimeSpan.FromSeconds(o.Timeout)));

            services.AddScoped<ApiClients>(sp => new ApiClients
            {
                CompanyDetailsApiClient = sp.GetRequiredService<ICompanyDetailsApiClient>(),
                SubmissionApiClient = sp.GetRequiredService<ISubmissionApiClient>(),
            });
        }

        private static Action<ResiliencePipelineBuilder<HttpResponseMessage>> BuildResiliencePipeline() =>
          builder => BuildResiliencePipeline(builder);

        private static Action<ResiliencePipelineBuilder<HttpResponseMessage>, ResilienceHandlerContext> BuildResiliencePipeline<TConfig>(Func<TConfig, TimeSpan> timeoutSelector)
            where TConfig : class =>
            (builder, context) =>
            {
                var sp = context.ServiceProvider;
                var timeout = timeoutSelector(sp.GetRequiredService<IOptions<TConfig>>()?.Value);
                BuildResiliencePipeline(builder, timeout);
            };

        private static void BuildResiliencePipeline(ResiliencePipelineBuilder<HttpResponseMessage> builder, TimeSpan? timeout = null)
        {
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = (RetryPredicateArguments<HttpResponseMessage> args) =>
                {
                    bool shouldHandle;
                    var exception = args.Outcome.Exception;
                    if (exception is TimeoutRejectedException ||
                       (exception is OperationCanceledException && exception.Source == "System.Private.CoreLib" && exception.InnerException is TimeoutException))
                    {
                        shouldHandle = true;
                    }
                    else
                    {
                        shouldHandle = HttpClientResiliencePredicates.IsTransient(args.Outcome);
                    }

                    return new ValueTask<bool>(shouldHandle);
                },
            });

            if (timeout is not null)
            {
                builder.AddTimeout(timeout.Value);
            }
        }
    }
}