namespace EPR.RegistrationValidation.Application.Providers;

using Exceptions;
using Newtonsoft.Json;

public class DequeueProvider : IDequeueProvider
{
    public T GetMessageFromJson<T>(string message)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<T>(message);
            return response;
        }
        catch (Exception e)
        {
            var errMsg = $"{typeof(T).Name} can not be deserialized.";
            throw new DeserializeQueueException(errMsg, e);
        }
    }
}