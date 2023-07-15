namespace EPR.RegistrationValidation.Functions.Extensions;

using System.Diagnostics.CodeAnalysis;
using Application.Extensions;
using Azure.Storage.Blobs;
using Data.Config;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[ExcludeFromCodeCoverage]
public static class ConfigurationExtensions
{
    public static IServiceCollection AddConfig(this IServiceCollection services)
    {
        services.ConfigureSection<ServiceBusConfig>("ServiceBus");
        services.ConfigureSection<StorageAccountConfig>("StorageAccount");
        services.ConfigureSection<SubmissionApiConfig>("SubmissionApi");

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