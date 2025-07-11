namespace EPR.RegistrationValidation.Application.Services;

using EPR.RegistrationValidation.Data.Config;
using Microsoft.Extensions.Options;

public class ValidationConfig
{
    public IOptions<ValidationSettings> ValidationSettings { get; set; }

    public IOptions<RegistrationSettings> RegistrationSettings { get; set; }
}