namespace EPR.RegistrationValidation.Data.Config;

using System.ComponentModel.DataAnnotations;

public class SubmissionApiConfig
{
    [Required]
    public string BaseUrl { get; init; }

    [Required]
    public string Version { get; init; }

    [Required]
    public string SubmissionEndpoint { get; init; }

    [Required]
    public string SubmissionEventEndpoint { get; init; }
}