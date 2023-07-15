namespace EPR.RegistrationValidation.Data.Config;

using System.ComponentModel.DataAnnotations;

public class StorageAccountConfig
{
    [Required]
    public string ConnectionString { get; init; }

    [Required]
    public string BlobContainerName { get; init; }
}