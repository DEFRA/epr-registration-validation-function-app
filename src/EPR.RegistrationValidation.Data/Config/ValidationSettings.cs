namespace EPR.RegistrationValidation.Data.Config;

public class ValidationSettings
{
    public const string Section = "ValidationSettings";

    public int ErrorLimit { get; set; } = 200;
}