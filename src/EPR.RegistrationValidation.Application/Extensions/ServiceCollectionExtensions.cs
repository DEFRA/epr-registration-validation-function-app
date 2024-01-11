namespace EPR.RegistrationValidation.Application.Extensions;

using System.Diagnostics.CodeAnalysis;
using Clients;
using EPR.RegistrationValidation.Application.Validators;
using Helpers;
using Microsoft.Extensions.DependencyInjection;
using Providers;
using Readers;
using Services;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // helpers
        services.AddSingleton<ColumnMetaDataProvider>();
        services.AddSingleton<ICsvStreamParser, CsvStreamParser>();

        // services
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IDequeueProvider, DequeueProvider>();
        services.AddScoped<IBlobReader, BlobReader>();
        services.AddScoped<ISubmissionApiClient, SubmissionApiClient>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<OrganisationDataRowValidator>();
        services.AddScoped<BrandDataRowValidator>();
        services.AddScoped<PartnerDataRowValidator>();

        return services;
    }
}