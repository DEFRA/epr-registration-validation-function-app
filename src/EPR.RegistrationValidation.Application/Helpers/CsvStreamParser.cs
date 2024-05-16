namespace EPR.RegistrationValidation.Application.Helpers;

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using EPR.RegistrationValidation.Application.ClassMaps;
using Exceptions;

public class CsvStreamParser : ICsvStreamParser
{
    private readonly ColumnMetaDataProvider _metaDataProvider;

    public CsvStreamParser(ColumnMetaDataProvider metaDataProvider)
    {
        _metaDataProvider = metaDataProvider;
    }

    private static CsvConfiguration CsvConfiguration => new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
    };

    public async Task<List<T>> GetItemsFromCsvStreamAsync<T>(MemoryStream memoryStream, bool useMinimalClassMaps = false)
    {
        try
        {
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            using var csv = new CsvReader(reader, CsvConfiguration);
            if (useMinimalClassMaps)
            {
                // Register class map to populate minimal set of properties to keep memory usage to a minimum
                csv.Context.RegisterClassMap<MinimalOrganisationDataRowMap>();
            }

            csv.Read();
            csv.ReadHeader();

            var header = csv.HeaderRecord;
            var columnAttributes = _metaDataProvider.ListColumnMetaData<T>();
            var orderedHeaders = columnAttributes
                .Select(x => x.Value)
                .OrderBy(x => x.Index)
                .ToList();

            if (!header.SequenceEqual(orderedHeaders.Select(x => x.Name)))
            {
                throw new CsvHeaderException("The CSV file header is invalid.");
            }

            var list = csv.GetRecords<T>().ToList();
            return list;
        }
        catch (CsvHeaderException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CsvParseException("Error parsing CSV", ex);
        }
    }
}