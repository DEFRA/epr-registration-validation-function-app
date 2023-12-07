namespace EPR.RegistrationValidation.Data.Config;

using System.ComponentModel.DataAnnotations;

public class ServiceBusConfig
{
    public const string Section = "ServiceBus";

    [Required]
    public string ConnectionString { get; init; }

    [Required]
    public string UploadQueueName { get; init; }
}