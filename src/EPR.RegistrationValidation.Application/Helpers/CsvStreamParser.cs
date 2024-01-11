namespace EPR.RegistrationValidation.Application.Helpers;

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Data.Models;
using EPR.RegistrationValidation.Application.ClassMaps;
using EPR.RegistrationValidation.Data.Constants;
using Exceptions;
using Microsoft.FeatureManagement;

public class CsvStreamParser : ICsvStreamParser
{
    private readonly ColumnMetaDataProvider _metaDataProvider;
    private readonly IFeatureManager _featureManager;

    public CsvStreamParser(ColumnMetaDataProvider metaDataProvider, IFeatureManager featureManager)
    {
        _metaDataProvider = metaDataProvider;
        _featureManager = featureManager;
    }

    private static CsvConfiguration CsvConfiguration => new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
    };

    public async Task<List<T>> GetItemsFromCsvStreamAsync<T>(MemoryStream memoryStream)
    {
        try
        {
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            using var csv = new CsvReader(reader, CsvConfiguration);
            if (await _featureManager.IsEnabledAsync(FeatureFlags.EnableRowValidation) == false)
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