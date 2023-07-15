namespace EPR.RegistrationValidation.Application.Helpers;

using System.Globalization;
using ClassMaps;
using CsvHelper;
using CsvHelper.Configuration;
using Exceptions;

public class CsvStreamParser : ICsvStreamParser
{
    private static CsvConfiguration CsvConfiguration => new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
    };

    public IList<T> GetItemsFromCsvStream<T>(MemoryStream memoryStream)
    {
        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream);
        using var csv = new CsvReader(reader, CsvConfiguration);
        try
        {
            csv.Context.RegisterClassMap<CsvDataRowMap>();
            var list = csv.GetRecords<T>().ToList();
            if (list.Count > 0)
            {
                return list;
            }

            throw new CsvParseException("No lines found");
        }
        catch (Exception ex)
        {
            throw new CsvParseException("Error parsing CSV", ex);
        }
    }
}