using System.Net.Http.Headers;
using EPR.RegistrationValidation.Application.Clients;
using EPR.RegistrationValidation.Application.Extensions;
using EPR.RegistrationValidation.Application.Handlers;
using EPR.RegistrationValidation.Application.Services;
using EPR.RegistrationValidation.Data.Config;
using EPR.RegistrationValidation.Functions.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Polly;
using Polly.Timeout;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddLogging();
        services.AddFeatureManagement();
        services.AddConfig();
        services.AddAzureClients();
        services.AddApplication();

        // Application Insights for isolated worker process
        services.AddApplicationInsightsTelemetryWorkerService();

        services.AddHttpClient<ISubmissionApiClient, SubmissionApiClient>((sp, c) =>
        {
            var submissionApiConfig = sp.GetRequiredService<IOptions<SubmissionApiConfig>>().Value;
            c.BaseAddress = new Uri(submissionApiConfig.BaseUrl);
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddResilienceHandler("SubmissionApiResiliencePipeline", builder => BuildResiliencePipeline(builder));

        services.AddHttpClient<ICompanyDetailsApiClient, CompanyDetailsApiClient>((sp, c) =>
        {
            var companyDetailsApiConfig = sp.GetRequiredService<IOptions<CompanyDetailsApiConfig>>().Value;
            c.BaseAddress = new Uri(companyDetailsApiConfig.BaseUrl);
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<CompanyDetailsApiAuthorisationHandler>()
        .AddResilienceHandler("CompanyDetailsResiliencePipeline", (builder, context) =>
        {
            var sp = context.ServiceProvider;
            var timeout = TimeSpan.FromSeconds(sp.GetRequiredService<IOptions<CompanyDetailsApiConfig>>().Value.Timeout);
            BuildResiliencePipeline(builder, timeout);
        });

        services.AddScoped<ApiClients>(sp => new ApiClients
        {
            CompanyDetailsApiClient = sp.GetRequiredService<ICompanyDetailsApiClient>(),
            SubmissionApiClient = sp.GetRequiredService<ISubmissionApiClient>(),
        });
    })
    .Build();

host.Run();

static void BuildResiliencePipeline(ResiliencePipelineBuilder<HttpResponseMessage> builder, TimeSpan? timeout = null)
{
    builder.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 4,
        Delay = TimeSpan.FromSeconds(2),
        BackoffType = DelayBackoffType.Exponential,
        ShouldHandle = args =>
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