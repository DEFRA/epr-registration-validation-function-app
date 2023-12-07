namespace EPR.RegistrationValidation.Data.Config;

using System.ComponentModel.DataAnnotations;

public class StorageAccountConfig
{
    public const string Section = "StorageAccount";

    [Required]
    public string ConnectionString { get; init; }

    [Required]
    public string BlobContainerName { get; init; }
}