namespace EPR.RegistrationValidation.Data.Config;

using System.ComponentModel.DataAnnotations;

public class CompanyDetailsApiConfig
{
    public const string Section = "CompanyDetailsApi";

    [Required]
    public string BaseUrl { get; init; }

    public string? ClientId { get; set; }

    public int Timeout { get; set; }
}