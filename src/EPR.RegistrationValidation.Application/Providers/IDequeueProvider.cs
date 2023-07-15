namespace EPR.RegistrationValidation.Application.Providers;

public interface IDequeueProvider
{
    public T GetMessageFromJson<T>(string message);
}