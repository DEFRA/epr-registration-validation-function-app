namespace EPR.RegistrationValidation.Application.Helpers;

public interface ICsvStreamParser
{
    Task<IList<T>> GetItemsFromCsvStreamAsync<T>(MemoryStream memoryStream);
}