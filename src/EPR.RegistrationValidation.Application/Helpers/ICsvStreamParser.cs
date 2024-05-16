namespace EPR.RegistrationValidation.Application.Helpers;

public interface ICsvStreamParser
{
    Task<List<T>> GetItemsFromCsvStreamAsync<T>(MemoryStream memoryStream, bool useMinimalClassMaps = false);
}