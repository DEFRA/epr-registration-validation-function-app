namespace EPR.RegistrationValidation.Functions.V1;

using Application.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class ServiceBusQueueTrigger
{
    private readonly IRegistrationService _registrationService;
    private readonly ILogger<ServiceBusQueueTrigger> _logger;

    public ServiceBusQueueTrigger(IRegistrationService registrationService, ILogger<ServiceBusQueueTrigger> logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    [Function("ServiceBusQueueTrigger")]
    public async Task RunAsync(
        [ServiceBusTrigger("%ServiceBus:UploadQueueName%", Connection = "ServiceBus:ConnectionString")]
        string message,
        FunctionContext context)
    {
        _logger.LogInformation("Entering function");

        await _registrationService.ProcessServiceBusMessage(message);

        _logger.LogInformation("Exiting function");
    }
}