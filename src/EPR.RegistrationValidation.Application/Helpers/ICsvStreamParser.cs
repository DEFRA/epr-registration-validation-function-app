namespace EPR.RegistrationValidation.Application.Helpers;

public interface ICsvStreamParser
{
    IList<T> GetItemsFromCsvStream<T>(MemoryStream memoryStream);
}