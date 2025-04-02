namespace EPR.RegistrationValidation.Functions.V1;

using Application.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

public class ServiceBusQueueTrigger
{
    private readonly IRegistrationService _registrationService;

    public ServiceBusQueueTrigger(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [FunctionName("ServiceBusQueueTrigger")]
    public async Task RunAsync(
        [ServiceBusTrigger("%ServiceBus:UploadQueueName%", Connection = "ServiceBus:ConnectionString")]
        string message,
        ILogger logger)
    {
        logger.LogInformation("Entering function");

        await _registrationService.ProcessServiceBusMessage(message);

        logger.LogInformation("Exiting function");
    }
}