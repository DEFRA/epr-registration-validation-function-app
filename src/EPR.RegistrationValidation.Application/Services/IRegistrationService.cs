namespace EPR.RegistrationValidation.Application.Services;

public interface IRegistrationService
{
    Task ProcessServiceBusMessage(string message);
}