namespace EPR.RegistrationValidation.Functions.Extensions;

using System.Diagnostics.CodeAnalysis;
using Application.Extensions;
using Azure.Storage.Blobs;
using Data.Config;
using EPR.RegistrationValidation.Application.Services;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[ExcludeFromCodeCoverage]
public static class ConfigurationExtensions
{
    public static IServiceCollection AddConfig(this IServiceCollection services)
    {
        services.ConfigureSection<CompanyDetailsApiConfig>(CompanyDetailsApiConfig.Section);
        services.ConfigureSection<ServiceBusConfig>(ServiceBusConfig.Section);
        services.ConfigureSection<StorageAccountConfig>(StorageAccountConfig.Section);
        services.ConfigureSection<SubmissionApiConfig>(SubmissionApiConfig.Section);
        services.ConfigureSection<ValidationSettings>(ValidationSettings.Section);
        services.ConfigureSection<RegistrationSettings>(RegistrationSettings.Section);

        services.AddSingleton<ValidationConfig>(sp => new ValidationConfig
        {
            ValidationSettings = sp.GetRequiredService<IOptions<ValidationSettings>>(),
            RegistrationSettings = sp.GetRequiredService<IOptions<RegistrationSettings>>(),
        });
        return services;
    }

    public static IServiceCollection AddAzureClients(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var serviceBusConfig = serviceProvider.GetRequiredService<IOptions<ServiceBusConfig>>();
        var storageAccountConfig = serviceProvider.GetRequiredService<IOptions<StorageAccountConfig>>();

        services.AddAzureClients(clientsBuilder =>
        {
            clientsBuilder.AddServiceBusClient(serviceBusConfig.Value.ConnectionString);
        });

        services.AddScoped(x => new BlobServiceClient(storageAccountConfig.Value.ConnectionString));
        return services;
    }
}